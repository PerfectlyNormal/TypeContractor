using System.Globalization;
using System.Reflection;
using TypeContractor.Logger;
using TypeContractor.TypeScript;

namespace TypeContractor;

public class TypeContractorConfiguration
{
    private static readonly string[] _defaultSuffixes = new[] { "Dto", "Request", "Response" };

    private readonly List<string> _suffixes = new();
    private readonly List<string> _types = new();
    private readonly Dictionary<string, string> _map = new();
    private readonly Dictionary<string, string> _assemblies = new();
    private readonly Dictionary<string, string> _replacements = new();
    private string? _outputPath;
    private ILog? _logger;

    public IReadOnlyDictionary<string, string> TypeMaps => _map;
    public IReadOnlyList<string> Suffixes => _suffixes.AsReadOnly();
    public IReadOnlyList<string> Types => _types.AsReadOnly();
    public IReadOnlyDictionary<string, string> Assemblies => _assemblies;
    public IReadOnlyDictionary<string, string> Replacements => _replacements;
    public string OutputPath => _outputPath ?? throw new InvalidOperationException("Output path is not configured");
    public ILog Logger => _logger ?? new NullLogger();

    /// <summary>
    /// Set up a default configuration using <see cref="AddDefaultSuffixes"/> and <see cref="AddDefaultTypeMaps"/>
    /// </summary>
    /// <returns></returns>
    public static TypeContractorConfiguration WithDefaultConfiguration()
    {
        return new TypeContractorConfiguration()
            .AddDefaultSuffixes()
            .AddDefaultTypeMaps();
    }

    /// <summary>
    /// Add default suffixes (<c>Dto</c>, <c>Request</c> and <c>Response</c>) to the list of classes
    /// to match and convert.
    /// 
    /// <para>To add custom suffixes, use <see cref="AddSuffix(string[])"/></para>
    /// </summary>
    /// <returns>The configuration object for continued chaining</returns>
    public TypeContractorConfiguration AddDefaultSuffixes()
    {
        _suffixes.AddRange(_defaultSuffixes);
        return this;
    }

    /// <summary>
    /// Adds mappings for the most common C# types to their TypeScript counterparts.
    /// 
    /// <para>
    /// To add custom mappings, use <see cref="AddCustomMap(Type, string)"/>
    /// or <see cref="AddCustomMap(string, string)"/>
    /// </para>
    /// </summary>
    /// <returns>The configuration object for continued chaining</returns>
    public TypeContractorConfiguration AddDefaultTypeMaps()
    {
        AddCustomMap(typeof(string), DestinationTypes.StringType);
        AddCustomMap(typeof(DateTime), DestinationTypes.StringType);
        AddCustomMap(typeof(DateTimeOffset), DestinationTypes.StringType);
        AddCustomMap(typeof(Guid), DestinationTypes.StringType);
        AddCustomMap(typeof(bool), DestinationTypes.Boolean);
        AddCustomMap(typeof(byte), DestinationTypes.Number);
        AddCustomMap(typeof(short), DestinationTypes.Number);
        AddCustomMap(typeof(int), DestinationTypes.Number);
        AddCustomMap(typeof(long), DestinationTypes.Number);
        AddCustomMap(typeof(decimal), DestinationTypes.Number);
        AddCustomMap(typeof(float), DestinationTypes.Number);
        AddCustomMap(typeof(Stream), DestinationTypes.ByteArray);
        AddCustomMap(typeof(CultureInfo), DestinationTypes.StringType);

        return this;
    }

    /// <summary>
    /// Add a custom mapping from a type.
    /// 
    /// <para>
    /// A list of default destination types can be found in <see cref="DestinationTypes"/>
    /// </para>
    /// 
    /// <para>
    /// If the type isn't available at compile time, use <see cref="AddCustomMap(string, string)"/> instead.
    /// </para>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="destinationType"></param>
    /// <returns>The configuration object for continued chaining</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="type"/> or <paramref name="destinationType"/> is null or empty</exception>
    /// <exception cref="InvalidOperationException">If the type being added already exists in the map</exception>
    public TypeContractorConfiguration AddCustomMap(Type type, string destinationType)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        return AddCustomMap(type.FullName!, destinationType);
    }

    /// <summary>
    /// Add a custom mapping from a type, specified as a string.
    /// 
    /// <para>
    /// A list of default destination types can be found in <see cref="DestinationTypes"/>
    /// </para>
    /// 
    /// <para>
    /// If the type is available at compile time, use <see cref="AddCustomMap(Type, string)"/> instead for improved
    /// type safety and resilience against refactoring and renames.
    /// </para>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="destinationType"></param>
    /// <returns>The configuration object for continued chaining</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="type"/> or <paramref name="destinationType"/> is null or empty</exception>
    /// <exception cref="InvalidOperationException">If the type being added already exists in the map</exception>
    public TypeContractorConfiguration AddCustomMap(string type, string destinationType)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentNullException(nameof(type));

        if (string.IsNullOrWhiteSpace(destinationType))
            throw new ArgumentNullException(nameof(destinationType));

        if (!_map.TryAdd(type, destinationType))
            throw new InvalidOperationException($"Unable to add {type} to list of maps. Already added?");

        return this;
    }

    /// <summary>
    /// Add a list of custom suffixes to filter classes by
    /// </summary>
    /// <param name="suffixes"></param>
    /// <returns></returns>
    public TypeContractorConfiguration AddSuffix(params string[] suffixes)
    {
        _suffixes.AddRange(suffixes);
        return this;
    }

    /// <summary>
    /// Add a list of types to explicitly include.
    /// 
    /// <para>
    /// Instead of automatically matching classes based on their suffix,
    /// it's possible to add certain types that should always be included.
    /// </para>
    /// 
    /// <para>
    /// Needs to be an exact match, similar to <c>typeof(T).FullName</c>,
    /// before replacements (see <see cref="AddReplacement(string, string)"/>)
    /// are applied.
    /// </para>
    /// </summary>
    /// <param name="types">A list of types to include</param>
    /// <returns>The configuration for further chaining</returns>
    public TypeContractorConfiguration AddTypes(params string[] types)
    {
        _types.AddRange(types);
        return this;
    }

    public TypeContractorConfiguration AddAssembly(string assemblyName, string assemblyPath)
    {
        if (!_assemblies.TryAdd(assemblyName, assemblyPath))
            throw new InvalidOperationException($"Unable to add {assemblyName}. Already added?");

        return this;
    }

    public TypeContractorConfiguration AddAssemblies(params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies, nameof(assemblies));
        foreach (var assembly in assemblies)
            AddAssembly(assembly.FullName!, assembly.Location);

        return this;
    }

    public TypeContractorConfiguration AddReplacement(string searchString, string replacement)
    {
        if (!_replacements.TryAdd(searchString, replacement))
            throw new InvalidOperationException($"Unable to add replacement {searchString}. Already added?");

        return this;
    }

    /// <summary>
    /// Add a string to be stripped from type names and folders.
    /// 
    /// <para>
    /// See also <see cref="AddReplacement(string, string)"/>
    /// </para>
    /// 
    /// <example>
    /// Example:
    /// 
    /// <code>
    /// .StripString("My.Deeply.Nested.Namespace")
    /// </code>
    /// 
    /// will turn <c>My.Deeply.Nested.Namespace.ClassName</c> into <c>ClassName</c>
    /// and placed directly under <c>&lt;output>/ClassName.ts</c> when the files are written.
    /// </example>
    /// </summary>
    /// <param name="searchString"></param>
    /// <returns>The configuration object for continued chaining</returns>
    /// <exception cref="InvalidOperationException">If <paramref name="searchString"/> already exists in the list of replacements</exception>
    public TypeContractorConfiguration StripString(string searchString) => AddReplacement(searchString, string.Empty);

    /// <summary>
    /// Set the output directory for files to be written to.
    /// 
    /// <example>
    /// For example:
    /// 
    /// <code>
    /// .SetOutputDirectory(Path.Combine(Directory.GetCurrentDirectory(), "App", "src", "api")));
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="outputDirectory"></param>
    /// <returns>The configuration object for continued chaining</returns>
    public TypeContractorConfiguration SetOutputDirectory(string outputDirectory)
    {
        _outputPath = outputDirectory;

        return this;
    }

    /// <summary>
    /// Add a logger of the provided type.
    /// 
    /// <para>
    /// An instance will be automatically created, and must have a parameterless constructor.
    /// If you have additional needs, create and provide an instance using <see cref="WithLogger{T}(T)"/> instead.
    /// </para>
    /// </summary>
    /// <typeparam name="T">A type implementing <see cref="ILog"/></typeparam>
    /// <returns>The configuration object for continued chaining</returns>
    public TypeContractorConfiguration WithLogger<T>() where T : ILog => WithLogger(Activator.CreateInstance<T>());

    /// <summary>
    /// Use the provided <see cref="ILog"/> instance as a logger
    /// </summary>
    /// <param name="logger">A logger instance</param>
    /// <typeparam name="T">A type implementing <see cref="ILog"/></typeparam>
    /// <returns>The configuration object for continued chaining</returns>
    public TypeContractorConfiguration WithLogger<T>(T logger) where T : ILog
    {
        _logger = logger;
        return this;
    }
}
