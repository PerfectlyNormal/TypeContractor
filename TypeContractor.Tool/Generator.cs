using System.Reflection;
using TypeContractor.Helpers;
using TypeContractor.Logger;
using TypeContractor.Output;
using static TypeContractor.Helpers.TypeChecks;

namespace TypeContractor.Tool;

internal class Generator
{
	private readonly string _assemblyPath;
	private readonly string _output;
	private readonly string? _relativeRoot;
	private readonly CleanMethod _cleanMethod;
	private readonly string[] _replacements;
	private readonly string[] _strip;
	private readonly string[] _customMaps;
	private readonly string _packPath;
	private readonly int _dotnetVersion;
	private readonly bool _buildZodSchemas;
	private readonly bool _generateApiClients;
	private readonly string _apiClientTemplate;

	public Generator(string assemblyPath,
					 string output,
					 string? relativeRoot,
					 CleanMethod cleanMethod,
					 string[] replacements,
					 string[] strip,
					 string[] customMaps,
					 string packsPath,
					 int dotnetVersion,
					 bool buildZodSchemas,
					 bool generateApiClients,
					 string apiClientTemplate)
	{
		_assemblyPath = assemblyPath;
		_output = output;
		_relativeRoot = relativeRoot;
		_cleanMethod = cleanMethod;
		_replacements = replacements;
		_strip = strip;
		_customMaps = customMaps;
		_packPath = packsPath;
		_dotnetVersion = dotnetVersion;
		_buildZodSchemas = buildZodSchemas;
		_generateApiClients = generateApiClients;
		_apiClientTemplate = apiClientTemplate;
	}

	public Task<int> Execute()
	{
		var returnCode = 0;

		MetadataLoadContext context;
		try
		{
			context = ReflectionContextHelper.GetMetadataContext(_packPath, _dotnetVersion, _assemblyPath, Log.Instance);
		}
		catch (DirectoryNotFoundException ex)
		{
			Log.Instance.LogError(ex, ex.Message);
			return Task.FromResult(1);
		}
		catch (FileNotFoundException ex)
		{
			Log.Instance.LogError(ex, ex.Message);
			return Task.FromResult(1);
		}

		try
		{
			Log.Instance.LogDebug($"Going to load assembly {_assemblyPath}");
			var assembly = context.LoadFromAssemblyPath(_assemblyPath);
			var controllers = assembly.GetTypes()
				.Where(IsController).ToList();
			var clients = new List<ApiClient>();

			if (controllers.Count == 0)
			{
				Log.Instance.LogError("Unable to find any controllers.");
				return Task.FromResult(1);
			}

			var typesToLoad = new Dictionary<Assembly, HashSet<Type>>();
			foreach (var controller in controllers)
			{
				Log.Instance.LogDebug($"Checking controller {controller.FullName}.");
				var endpoints = controller.GetMethods()
					.Where(ReturnsActionResult).ToList();

				var returnTypes = endpoints
					.Select(FullyUnwrappedReturnType).Where(x => x != null)
					.Cast<Type>().ToList();

				var parameterTypes = endpoints
					.SelectMany(UnwrappedParameters).Where(x => x != null)
					.Cast<Type>().ToList();

				if (_generateApiClients)
				{
					Log.Instance.LogDebug($"Generating endpoints for {controller.FullName}");
					var client = ApiHelpers.BuildApiClient(controller, endpoints);

					if (client?.Endpoints.Any() ?? false)
					{
						if (clients.Any(x => x.Name == client.Name))
						{
							var originalName = client.Name;
							var newName = string.Join("", controller.FullName!.Split('.')[^2..]);
							newName = $"{newName[0..1].ToUpper()}{newName[1..]}";

							Log.Instance.LogWarning($"Found existing client with name {originalName}. Will rename to {newName}");
							client = client with { Name = newName };
						}

						clients.Add(client);
					}
				}

				foreach (var returnType in returnTypes)
				{
					Log.Instance.LogDebug($"Adding (return) type {returnType.FullName} from assembly {returnType.Assembly.FullName}");
					typesToLoad.TryAdd(returnType.Assembly, []);
					typesToLoad[returnType.Assembly].Add(returnType);
				}

				foreach (var parameterType in parameterTypes)
				{
					Log.Instance.LogDebug($"Adding (parameter) type {parameterType.FullName} from assembly {parameterType.Assembly.FullName}");
					typesToLoad.TryAdd(parameterType.Assembly, []);
					typesToLoad[parameterType.Assembly].Add(parameterType);
				}
			}

			if (typesToLoad.Count == 0)
			{
				Log.Instance.LogWarning("Unable to find any types to convert that matches the expected format.");
				return Task.FromResult(1);
			}

			var contractor = GenerateContractor(typesToLoad, clients);

			if (_cleanMethod == CleanMethod.Remove)
			{
				Log.Instance.LogWarning($"Going to clean output path '{_output}'.");
				if (Directory.Exists(_output))
				{
					Directory.Delete(_output, true);
					Directory.CreateDirectory(_output);
				}
			}

			Log.Instance.LogMessage("Writing types.");
			returnCode = contractor.Build(context, _cleanMethod == CleanMethod.Smart);
			Log.Instance.LogMessage("Finished generating types.");
		}
		catch (FileLoadException ex)
		{
			Log.Instance.LogError(ex, string.Format("Unable to load a file. Loaded assemblies are: {0}", string.Join(",\n", context.GetAssemblies().Select(ass => ass.FullName))));
			returnCode = 1;
		}

		return Task.FromResult(returnCode);
	}

	private Contractor GenerateContractor(Dictionary<Assembly, HashSet<Type>> typesToLoad, List<ApiClient> clients)
	{
		var configuration = new TypeContractorConfiguration()
							.AddDefaultTypeMaps()
							.AddAssemblies([.. typesToLoad.Keys])
							.AddTypes(typesToLoad.Values.SelectMany(list => list.Select(t => t.FullName!)).ToArray())
							.AddApiClients(clients, _apiClientTemplate)
							.SetOutputDirectory(_output!);

		if (!string.IsNullOrWhiteSpace(_relativeRoot))
			configuration = configuration.SetRelativeRoot(_relativeRoot);

		if (_buildZodSchemas)
			configuration = configuration.EnableZodSchemas();

		if (_strip is not null)
			foreach (var strip in _strip)
				configuration = configuration.StripString(strip);

		if (_replacements is not null)
			foreach (var task in _replacements)
			{
				var parts = task.Split(':').Select(x => x.Trim()).ToList();
				if (parts.Count != 2)
				{
					Log.Instance.LogWarning($"Unable to parse '{task}' into a replacement. Syntax is 'search:replacement'.");
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
					Log.Instance.LogWarning($"Unable to parse '{task}' into a custom type map. Syntax is 'sourceTypeWithNamespace:destinationTypeWithNamespace'.");
					continue;
				}

				configuration = configuration.AddCustomMap(parts.ElementAt(0), parts.ElementAt(1));
			}

		return Contractor.WithConfiguration(configuration);
	}
}
