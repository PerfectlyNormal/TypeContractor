using System.Reflection;
using System.Runtime.InteropServices;
using TypeContractor.Logger;
using TypeContractor.Output;
using TypeContractor.TypeScript;

namespace TypeContractor;

public class Contractor
{
    private readonly TypeContractorConfiguration _configuration;

    public static Contractor WithConfiguration(TypeContractorConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        return new Contractor(configuration);
    }

    /// <summary>
    /// Construct a <see cref="Contractor"/> using <see cref="TypeContractorConfiguration.WithDefaultConfiguration"/>
    /// and provide an action to continue configuring Contractor.
    /// </summary>
    /// <param name="configurationBuilder"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configurationBuilder"/> is null</exception>
    public static Contractor FromDefaultConfiguration(Action<TypeContractorConfiguration> configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder, nameof(configurationBuilder));

        var configuration = TypeContractorConfiguration.WithDefaultConfiguration();
        configurationBuilder(configuration);

        return new Contractor(configuration);
    }

    private Contractor(TypeContractorConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TypeContractorConfiguration Configuration => _configuration;

    public int Build(MetadataLoadContext? metadataLoadContext = null, bool smartClean = false)
    {
        var returnCode = 0;

        metadataLoadContext ??= BuildMetadataLoadContext();
        var toConvert = new List<ContractedType>();
        var generatedFiles = new List<string>();
        var allTypes = new List<OutputType>();

        var converter = new TypeScriptConverter(_configuration, metadataLoadContext);
        var writer = new TypeScriptWriter(_configuration.OutputPath);

        foreach (var (assemblyName, assemblyPath) in _configuration.Assemblies)
        {
            var assembly = metadataLoadContext.LoadFromAssemblyPath(assemblyPath);

            var types = assembly.GetTypes()
                .Where(t => _configuration.Types.Any(type => type == t.FullName) || _configuration.Suffixes.Any(suffix => t.Name.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase)))
                .Select(t => ContractedType.FromName(t.FullName!, t, _configuration))
                .ToList();

            var folders = types
                .Select(t => t.Folder)
                .DistinctBy(x => x.Path)
                .OrderBy(x => x.Path)
                .ToList();

            foreach (var folder in folders)
            {
                var items = types
                    .Where(n => n.Folder == folder);

                toConvert.AddRange(items);
            }

            var outputTypes = types
                .Select(converter.Convert)
                .ToList() // Needed so `converter.Convert` runs before we concat
                .Concat(converter.CustomMappedTypes.Values)
                .ToList();

            allTypes.AddRange(outputTypes);

            foreach (var type in outputTypes)
            {
                try
                {
                    var filePath = writer.Write(type, outputTypes, Configuration.BuildZodSchemas);
                    generatedFiles.Add(filePath);
                }
                catch (TypeScriptReferenceException ex)
                {
                    var references = ex.References
                        .SelectMany(r => r.Properties.Select(x => $"\n- {r.Type.Name}.{x.SourceName} ({r.Type.FullName})"))
                        .ToList();

                    Log.Instance.LogError(ex, 
                        $"Unable to generate typings for {type.Name} ({type.FullName}):\n" +
                        $"{ex.Message}\n" +
                        (ex.PropertyWithError is not null ? $"Full type of property: {ex.PropertyWithError.SourceType.FullName}\n\n" : "")+
                        $"{type.Name} is referenced by: {string.Join("", references)}\n");

                    returnCode = 1;
                }
                catch (TypeScriptImportException ex)
                {
                    Log.Instance.LogError(ex, $"Unable to generate typings for {type.Name} ({type.FullName})");

                    returnCode = 1;
                }
            }
        }

        if (Configuration.GenerateApiClients)
        {
            var apiWriter = new ApiClientWriter(_configuration.OutputPath, _configuration.RelativeRoot);
            foreach (var client in Configuration.ApiClients)
            {
                try
                {
                    var filePath = apiWriter.Write(client, allTypes, converter, Configuration.BuildZodSchemas);
                    generatedFiles.Add(filePath);
                }
                catch (TypeScriptReferenceException ex)
                {
                    Log.Instance.LogError(ex,
                        $"Unable to generate client for {client.Name} ({client.TypeName}):\n" + ex.Message);

                    returnCode = 1;
                }
            }
        }

        if (smartClean)
        {
            Log.Instance.LogMessage("Cleaning no longer relevant output files.");
            var allFiles = Directory.GetFiles(_configuration.OutputPath, "*", SearchOption.AllDirectories);
            var diff = allFiles.Except(generatedFiles).ToList();

            var allDirectories = Directory.GetDirectories(_configuration.OutputPath, "*", SearchOption.AllDirectories);
            var generatedDirectories = allDirectories.Where(d => generatedFiles.Any(f => f.StartsWith(d, StringComparison.InvariantCultureIgnoreCase)));
            var directoryDiff = allDirectories.Except(generatedDirectories).ToList();

            Log.Instance.LogDebug($"Found a diff of {diff.Count} items:");
            foreach (var file in diff)
            {
                Log.Instance.LogDebug($"  {file}");
                try
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                catch (IOException ex)
                {
                    Log.Instance.LogWarning($"Unable to delete file {file}: {ex.Message}");
                }
            }

            Log.Instance.LogDebug($"Found a diff of {directoryDiff.Count} folders:");
            foreach (var dir in directoryDiff)
            {
                Log.Instance.LogDebug($"  {dir}");
                try
                {
                    if (Directory.Exists(dir))
                        Directory.Delete(dir, true);
                }
                catch (IOException ex)
                {
                    Log.Instance.LogWarning($"Unable to delete directory {dir}: {ex.Message}");
                }
            }
        }

        return returnCode;
    }

    private MetadataLoadContext BuildMetadataLoadContext()
    {
        // Get the array of runtime assemblies.
        var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");

        // Create the list of assembly paths consisting of runtime assemblies and the inspected assemblies.
        var paths = runtimeAssemblies.Concat(_configuration.Assemblies.Values);

        var resolver = new PathAssemblyResolver(paths);

        return new MetadataLoadContext(resolver);
    }
}
