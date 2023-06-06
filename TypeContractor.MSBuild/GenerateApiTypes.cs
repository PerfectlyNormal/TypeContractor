using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using TypeContractor.Helpers;

namespace TypeContractor.MSBuild
{
    public class GenerateApiTypes : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string? TypesOutputPath { get; set; }

        [Required]
        public string? AssemblyPath { get; set; }

        public string? CleanOutputPath { get; set; }

        public ITaskItem[]? Replacements { get; set; }

        public ITaskItem[]? StripStrings { get; set; }

        public ITaskItem[]? CustomTypeMaps { get; set; }

        public override bool Execute()
        {
            try
            {
                Log.LogMessage($"Going to generate types, starting with {AssemblyPath}.");
                var context = GetMetadataContext();
                var assembly = context.LoadFromAssemblyPath(AssemblyPath!);
                var controllers = assembly
                    .GetTypes().Where(IsController)
                    .ToList();

                if (!controllers.Any())
                {
                    Log.LogError("Unable to find any controllers");
                    return true;
                }

                var typesToLoad = new Dictionary<Assembly, HashSet<Type>>();
                foreach (var controller in controllers)
                {
                    Log.LogMessage($"Checking controller {controller.FullName}");
                    var returnTypes = controller
                        .GetMethods().Where(ReturnsActionResult)
                        .Select(UnwrappedResult).Where(x => x != null)
                        .ToList();

                    foreach (var returnType in returnTypes)
                    {
                        if (returnType is null)
                            continue;

                        typesToLoad.TryAdd(returnType.Assembly, new HashSet<Type>());
                        typesToLoad[returnType.Assembly].Add(returnType);
                    }
                }

                if (!typesToLoad.Any())
                {
                    Log.LogWarning("Unable to find any types to convert that matches the expected format.");
                    return true;
                }

                var contractor = GenerateContractor(typesToLoad);

                if (!string.IsNullOrWhiteSpace(CleanOutputPath) && bool.TryParse(CleanOutputPath, out var clean))
                    if (clean)
                    {
                        Log.LogWarning($"Going to clean output path '{TypesOutputPath}'");
                        if (Directory.Exists(TypesOutputPath))
                        {
                            Directory.Delete(TypesOutputPath, true);
                            Directory.CreateDirectory(TypesOutputPath);
                        }
                    }

                contractor.Build(context);

                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, showStackTrace: true);
                return true;
            }
        }

        private Contractor GenerateContractor(Dictionary<Assembly, HashSet<Type>> typesToLoad)
        {
            var configuration = new TypeContractorConfiguration()
                                .AddDefaultTypeMaps()
                                .AddAssemblies(typesToLoad.Keys.ToArray())
                                .AddTypes(typesToLoad.Values.SelectMany(list => list.Select(t => t.FullName!)).ToArray())
                                .SetOutputDirectory(TypesOutputPath!);

            if (StripStrings is not null)
                foreach (var strip in StripStrings)
                    configuration = configuration.StripString(strip.ItemSpec);

            if (Replacements is not null)
                foreach (var task in Replacements)
                {
                    var parts = task.ItemSpec.Split(':').Select(x => x.Trim()).ToList();
                    if (parts.Count != 2)
                    {
                        Log.LogWarning($"Unable to parse '{task.ItemSpec}' into a replacement. Syntax is 'search:replacement'.");
                        continue;
                    }

                    configuration = configuration.AddReplacement(parts.ElementAt(0), parts.ElementAt(1));
                }

            if (CustomTypeMaps is not null)
                foreach (var task in CustomTypeMaps)
                {
                    var parts = task.ItemSpec.Split(':').Select(x => x.Trim()).ToList();
                    if (parts.Count != 2)
                    {
                        Log.LogWarning($"Unable to parse '{task.ItemSpec}' into a custom type map. Syntax is 'sourceTypeWithNamespace:destinationTypeWithNamespace'.");
                        continue;
                    }

                    configuration = configuration.AddCustomMap(parts.ElementAt(0), parts.ElementAt(1));
                }

            return Contractor.WithConfiguration(configuration);
        }

        private MetadataLoadContext GetMetadataContext() => new(GetResolver());

        private PathAssemblyResolver GetResolver()
        {
            // Get the array of runtime assemblies.
            var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");

            // Get the ASP.NET Core assemblies
            const string aspnetPrefix = @"C:\Program Files\dotnet\packs\Microsoft.AspNetCore.App.Ref";
            var aspnetDirectories = Directory.EnumerateDirectories(aspnetPrefix, "6.0.*");
            var aspnetDirectory = aspnetDirectories.OrderByDescending(x => x).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(aspnetDirectory))
                throw new FileNotFoundException($"Unable to find Microsoft.AspNetCore.App references. Searched in {aspnetPrefix}.");
            aspnetDirectory = Path.Combine(aspnetDirectory, "ref", "net6.0");
            var aspnetAssemblies = Directory.GetFiles(aspnetDirectory, "*.dll");

            // Get the app-specific assemblies
            var appAssemblies = Directory.GetFiles(Path.GetDirectoryName(AssemblyPath)!, "*.dll");

            // Create the list of assembly paths consisting of runtime assemblies and the inspected assembly.
            var paths = runtimeAssemblies.Concat(aspnetAssemblies).Concat(appAssemblies);

            return new PathAssemblyResolver(paths);
        }

        private bool IsController(Type type)
        {
            if (type.FullName == "Microsoft.AspNetCore.Mvc.ControllerBase")
                return true;

            if (type.BaseType is not null)
                return IsController(type.BaseType);

            return false;
        }

        private bool ReturnsActionResult(MethodInfo methodInfo)
        {
            if (methodInfo.ReturnType.Name == "ActionResult`1")
                return true;

            if (methodInfo.ReturnType.Name == "Task`1" && methodInfo.ReturnType.GenericTypeArguments.Any(rt => rt.Name == "ActionResult`1"))
                return true;

            return false;
        }

        private Type? UnwrappedResult(MethodInfo methodInfo)
        {
            if (methodInfo.ReturnType.Name == "Task`1")
                return UnwrappedResult(methodInfo.ReturnType.GenericTypeArguments.First());

            if (methodInfo.ReturnType.Name == "ActionResult`1")
                return UnwrappedResult(methodInfo.ReturnType.GenericTypeArguments.First());

            return null;
        }

        private Type? UnwrappedResult(Type type)
        {
            if (type.Name == "ActionResult`1")
                return UnwrappedResult(type.GenericTypeArguments[0]);

            if (TypeChecks.ImplementsIEnumerable(type))
                return UnwrappedResult(type.GenericTypeArguments[0]);

            if (type.FullName!.StartsWith("System.", StringComparison.InvariantCulture))
                return null;

            return type;
        }
    }
}
