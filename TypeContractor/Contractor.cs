using System.Reflection;
using System.Runtime.InteropServices;
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

    public void Build(MetadataLoadContext? metadataLoadContext = null, bool smartClean = false)
    {
        metadataLoadContext ??= BuildMetadataLoadContext();
        var toConvert = new List<ContractedType>();
        var generatedFiles = new List<string>();

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

            var converter = new TypeScriptConverter(_configuration, metadataLoadContext);
            var writer = new TypeScriptWriter(_configuration.OutputPath);

            var outputTypes = types
                .Select(converter.Convert)
                .ToList() // Needed so `converter.Convert` runs before we concat
                .Concat(converter.CustomMappedTypes.Values)
                .ToList();

            foreach (var type in outputTypes)
            {
                var filePath = writer.Write(type, outputTypes);
                generatedFiles.Add(filePath);
            }
        }

        if (smartClean)
        {
            _configuration.Logger.LogMessage("Cleaning no longer relevant output files.");
            var allFiles = Directory.GetFiles(_configuration.OutputPath, "*", SearchOption.AllDirectories);
            var diff = allFiles.Except(generatedFiles).ToList();

            var allDirectories = Directory.GetDirectories(_configuration.OutputPath, "*", SearchOption.AllDirectories);
            var generatedDirectories = allDirectories.Where(d => generatedFiles.Any(f => f.StartsWith(d, StringComparison.InvariantCultureIgnoreCase)));
            var directoryDiff = allDirectories.Except(generatedDirectories).ToList();

            _configuration.Logger.LogDebug($"Found a diff of {diff.Count} items:");
            foreach (var file in diff)
            {
                _configuration.Logger.LogDebug($"  {file}");
                try
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                catch (IOException ex)
                {
                    _configuration.Logger.LogWarning($"Unable to delete file {file}: {ex.Message}");
                }
            }

            _configuration.Logger.LogDebug($"Found a diff of {directoryDiff.Count} folders:");
            foreach (var dir in directoryDiff)
            {
                _configuration.Logger.LogDebug($"  {dir}");
                try
                {
                    if (Directory.Exists(dir))
                        Directory.Delete(dir, true);
                }
                catch (IOException ex)
                {
                    _configuration.Logger.LogWarning($"Unable to delete directory {dir}: {ex.Message}");
                }
            }
        }
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
