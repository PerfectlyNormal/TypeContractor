using System.Reflection;
using TypeContractor.Logger;
using static TypeContractor.Helpers.TypeChecks;

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
    private readonly ILog _logger;

    public Generator(string assemblyPath, string output, bool clean, string[] replacements, string[] strip, string[] customMaps, string packsPath, ILog logger)
    {
        _assemblyPath = assemblyPath;
        _output = output;
        _clean = clean;
        _replacements = replacements;
        _strip = strip;
        _customMaps = customMaps;
        _packPath = packsPath;
        _logger = logger;
    }

    public Task Execute()
    {
        MetadataLoadContext context;
        try
        {
            context = ReflectionContextHelper.GetMetadataContext(_packPath, _assemblyPath, _logger);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, ex.Message);
            return Task.CompletedTask;
        }

        var assembly = context.LoadFromAssemblyPath(_assemblyPath);
        var controllers = assembly.GetTypes()
            .Where(IsController).ToList();

        if (!controllers.Any())
        {
            _logger.LogError("Unable to find any controllers.");
            return Task.CompletedTask;
        }

        var typesToLoad = new Dictionary<Assembly, HashSet<Type>>();
        foreach (var controller in controllers)
        {
            _logger.LogDebug($"Checking controller {controller.FullName}.");
            var endpoints = controller.GetMethods()
                .Where(ReturnsActionResult).ToList();

            var returnTypes = endpoints
                .Select(UnwrappedReturnType).Where(x => x != null)
                .Cast<Type>().ToList();

            var parameterTypes = endpoints
                .SelectMany(UnwrappedParameters).Where(x => x != null)
                .Cast<Type>().ToList();

            foreach (var returnType in returnTypes)
            {
                _logger.LogDebug($"Adding (return) type {returnType.FullName} from assembly {returnType.Assembly.FullName}");
                typesToLoad.TryAdd(returnType.Assembly, new HashSet<Type>());
                typesToLoad[returnType.Assembly].Add(returnType);
            }

            foreach (var parameterType in parameterTypes)
            {
                _logger.LogDebug($"Adding (parameter) type {parameterType.FullName} from assembly {parameterType.Assembly.FullName}");
                typesToLoad.TryAdd(parameterType.Assembly, new HashSet<Type>());
                typesToLoad[parameterType.Assembly].Add(parameterType);
            }
        }

        if (!typesToLoad.Any())
        {
            _logger.LogWarning("Unable to find any types to convert that matches the expected format.");
            return Task.CompletedTask;
        }

        var contractor = GenerateContractor(typesToLoad);

        if (_clean)
        {
            _logger.LogWarning($"Going to clean output path '{_output}'.");
            if (Directory.Exists(_output))
            {
                Directory.Delete(_output, true);
                Directory.CreateDirectory(_output);
            }
        }

        _logger.LogMessage("Writing types.");
        contractor.Build(context);
        _logger.LogMessage("Finished generating types.");

        return Task.CompletedTask;
    }

    private Contractor GenerateContractor(Dictionary<Assembly, HashSet<Type>> typesToLoad)
    {
        var configuration = new TypeContractorConfiguration()
                            .AddDefaultTypeMaps()
                            .AddAssemblies(typesToLoad.Keys.ToArray())
                            .AddTypes(typesToLoad.Values.SelectMany(list => list.Select(t => t.FullName!)).ToArray())
                            .SetOutputDirectory(_output!)
                            .WithLogger(_logger);

        if (_strip is not null)
            foreach (var strip in _strip)
                configuration = configuration.StripString(strip);

        if (_replacements is not null)
            foreach (var task in _replacements)
            {
                var parts = task.Split(':').Select(x => x.Trim()).ToList();
                if (parts.Count != 2)
                {
                    _logger.LogWarning($"Unable to parse '{task}' into a replacement. Syntax is 'search:replacement'.");
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
                    _logger.LogWarning($"Unable to parse '{task}' into a custom type map. Syntax is 'sourceTypeWithNamespace:destinationTypeWithNamespace'.");
                    continue;
                }

                configuration = configuration.AddCustomMap(parts.ElementAt(0), parts.ElementAt(1));
            }

        return Contractor.WithConfiguration(configuration);
    }
}
