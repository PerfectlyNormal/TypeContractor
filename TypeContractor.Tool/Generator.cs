using System.Reflection;
using System.Runtime.InteropServices;
using TypeContractor.Helpers;

namespace TypeContractor.Tool;

internal class Generator
{
    private readonly string _assemblyPath;
    private readonly string _output;
    private readonly bool _clean;
    private readonly string[] _replacements;
    private readonly string[] _strip;
    private readonly string[] _customMaps;
    private readonly string _packPath;

    public Generator(string assemblyPath, string output, bool clean, string[] replacements, string[] strip, string[] customMaps, string packsPath)
    {
        _assemblyPath = assemblyPath;
        _output = output;
        _clean = clean;
        _replacements = replacements;
        _strip = strip;
        _customMaps = customMaps;
        _packPath = packsPath;
    }

    public Task Execute()
    {
        MetadataLoadContext context;
        try
        {
            context = GetMetadataContext();
        }
        catch (FileNotFoundException ex)
        {
            Log.LogError(ex, ex.Message);
            return Task.CompletedTask;
        }

        var assembly = context.LoadFromAssemblyPath(_assemblyPath);
        var controllers = assembly
            .GetTypes().Where(IsController)
            .ToList();

        if (!controllers.Any())
        {
            Log.LogError("Unable to find any controllers.");
            return Task.CompletedTask;
        }

        var typesToLoad = new Dictionary<Assembly, HashSet<Type>>();
        foreach (var controller in controllers)
        {
            Log.LogMessage($"Checking controller {controller.FullName}.");
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
            return Task.CompletedTask;
        }

        var contractor = GenerateContractor(typesToLoad);

        if (_clean)
        {
            Log.LogWarning($"Going to clean output path '{_output}'.");
            if (Directory.Exists(_output))
            {
                Directory.Delete(_output, true);
                Directory.CreateDirectory(_output);
            }
        }

        Log.LogMessage("Writing types.");
        contractor.Build(context);
        Log.LogMessage("Finished generating types.");

        return Task.CompletedTask;
    }

    private Contractor GenerateContractor(Dictionary<Assembly, HashSet<Type>> typesToLoad)
    {
        var configuration = new TypeContractorConfiguration()
                            .AddDefaultTypeMaps()
                            .AddAssemblies(typesToLoad.Keys.ToArray())
                            .AddTypes(typesToLoad.Values.SelectMany(list => list.Select(t => t.FullName!)).ToArray())
                            .SetOutputDirectory(_output!);

        if (_strip is not null)
            foreach (var strip in _strip)
                configuration = configuration.StripString(strip);

        if (_replacements is not null)
            foreach (var task in _replacements)
            {
                var parts = task.Split(':').Select(x => x.Trim()).ToList();
                if (parts.Count != 2)
                {
                    Log.LogWarning($"Unable to parse '{task}' into a replacement. Syntax is 'search:replacement'.");
                    continue;
                }

                configuration = configuration.AddReplacement(parts.ElementAt(0), parts.ElementAt(1));
            }

        if (_customMaps is not null)
            foreach (var task in _customMaps)
            {
                var parts = task.Split(':').Select(x => x.Trim()).ToList();
                if (parts.Count != 2)
                {
                    Log.LogWarning($"Unable to parse '{task}' into a custom type map. Syntax is 'sourceTypeWithNamespace:destinationTypeWithNamespace'.");
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
        var runtimeFiles = runtimeAssemblies.Select(ass => Path.GetFileName(ass)).ToList();

        // Get the .NET Core assemblies
        var netcoreDirectory = GetNetCorePack("Microsoft.NETCore.App.Ref")
            ?? throw new FileNotFoundException($"Unable to find Microsoft.NETCore.App.Ref v6.0.x references. Searched in {_packPath}.");
        var netcoreAssemblies = Directory
            .GetFiles(netcoreDirectory, "*.dll")
            .Where(ass => !runtimeFiles.Contains(Path.GetFileName(ass)));

        // Get the ASP.NET Core assemblies
        var aspnetDirectory = GetNetCorePack("Microsoft.AspNetCore.App.Ref")
            ?? throw new FileNotFoundException($"Unable to find Microsoft.AspNetCore.App.Ref v6.0.x references. Searched in {_packPath}.");
        var aspnetAssemblies = Directory.GetFiles(aspnetDirectory, "*.dll");

        // Get the app-specific assemblies
        var appAssemblies = Directory.GetFiles(Path.GetDirectoryName(_assemblyPath)!, "*.dll");

        // Create the list of assembly paths consisting of runtime assemblies and the inspected assembly.
        var paths = runtimeAssemblies.Concat(netcoreAssemblies).Concat(aspnetAssemblies).Concat(appAssemblies);

        return new PathAssemblyResolver(paths);
    }

    private string? GetNetCorePack(string packName)
    {
        var packPrefix = @$"{_packPath}\{packName}";
        if (!Directory.Exists(packPrefix))
            return null;

        var availablePacks = Directory.EnumerateDirectories(packPrefix, "6.0.*");
        var packDirectory = availablePacks.OrderByDescending(x => x).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(packDirectory))
            return null;
        
        packDirectory = Path.Combine(packDirectory, "ref", "net6.0");
        if (!Directory.Exists(packDirectory))
            return null;

        return packDirectory;
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
