using DotNetConfig;
using System.CommandLine;
using TypeContractor;
using TypeContractor.Logger;
using TypeContractor.Tool;

var config = Config.Build("typecontractor.config");

var rootCommand = new RootCommand("Tool for generating TypeScript definitions from C# code");

var assemblyOption = new Option<string>("--assembly")
{
	Description = "Path to the assembly to start with. Will be relative to the current directory",
	Required = true,
};

var outputOption = new Option<string>("--output")
{
	Description = "Output path to write to. Will be relative to the current directory",
	Required = true,
};

var relativeRootOption = new Option<string>("--root")
{
	Description = "Relative root for generating cleaner imports. For example '~/api'",
};

var cleanOption = new Option<CleanMethod>("--clean")
{
	DefaultValueFactory = (arg) => CleanMethod.Smart,
	Description = "Choose how to clean up no longer relevant type files in output directory. Danger!",
};

var replaceOptions = new Option<string[]>("--replace")
{
	Description = "Provide one replacement in the form '<search>:<replace>'. Can be repeated",
};

var stripOptions = new Option<string[]>("--strip")
{
	Description = "Provide a prefix to strip out of types. Can be repeated",
};

var mapOptions = new Option<string[]>("--custom-map")
{
	Description = "Provide a custom type map in the form '<from>:<to>'. Can be repeated",
};

var packsOptions = new Option<string>("--packs-path")
{
	DefaultValueFactory = (arg) => @"C:\Program Files\dotnet\packs\",
	Description = "Path where dotnet is installed and reference assemblies can be found.",
};

var dotnetVersionOptions = new Option<int>("--dotnet-version")
{
	DefaultValueFactory = (arg) => 8,
	Description = "Major version of dotnet to look for",
};

var logLevelOptions = new Option<LogLevel>("--log-level")
{
	DefaultValueFactory = (arg) => LogLevel.Info,
};

var buildZodSchemasOptions = new Option<bool>("--build-zod-schemas")
{
	DefaultValueFactory = (arg) => false,
	Description = "Enable experimental support for Zod schemas alongside generated types.",
};

var generateApiClientsOptions = new Option<bool>("--generate-api-clients")
{
	DefaultValueFactory = (arg) => false,
	Description = "Enable experimental support for auto-generating API clients for each endpoint.",
};

var apiClientsTemplateOptions = new Option<string>("--api-client-template")
{
	DefaultValueFactory = (arg) => "aurelia",
	Description = "Template to use for API clients. Either 'aurelia', 'react-axios' (built-in) or a path to a Handlebars file, including extension",
};

var casingOptions = new Option<Casing>("--casing")
{
	DefaultValueFactory = (arg) => Casing.Kebab,
	Description = "Casing to use for generated file names",
};

rootCommand.Options.Add(assemblyOption);
rootCommand.Options.Add(outputOption);
rootCommand.Options.Add(relativeRootOption);
rootCommand.Options.Add(cleanOption);
rootCommand.Options.Add(replaceOptions);
rootCommand.Options.Add(stripOptions);
rootCommand.Options.Add(mapOptions);
rootCommand.Options.Add(packsOptions);
rootCommand.Options.Add(dotnetVersionOptions);
rootCommand.Options.Add(logLevelOptions);
rootCommand.Options.Add(buildZodSchemasOptions);
rootCommand.Options.Add(generateApiClientsOptions);
rootCommand.Options.Add(apiClientsTemplateOptions);
rootCommand.Options.Add(casingOptions);

apiClientsTemplateOptions.Validators.Add(result =>
{
	var value = result.GetValue(apiClientsTemplateOptions)!;
	if (value.Equals("aurelia", StringComparison.CurrentCultureIgnoreCase) || value.Equals("react-axios", StringComparison.CurrentCultureIgnoreCase))
		return;

	var generateClients = result.GetValue(generateApiClientsOptions);
	if (!generateClients)
	{
		result.AddError($"Must generate API clients for --{apiClientsTemplateOptions.Name} to have any effect.");
		return;
	}

	if (!File.Exists(value))
	{
		result.AddError($"The template specified does not exist or is not readable. Searched for {Path.GetFullPath(Path.Join(Directory.GetCurrentDirectory(), value))}.");
		return;
	}
});

// Apply configuration from file, if any
rootCommand = rootCommand.WithConfigurableDefaults("typecontractor", config);

rootCommand.SetAction(async (parseResult, cancellationToken) =>
{
	var assemblyOptionValue = parseResult.GetValue(assemblyOption)!;
	var outputValue = parseResult.GetValue(outputOption)!;
	var relativeRootValue = parseResult.GetValue(relativeRootOption);
	var cleanValue = parseResult.GetValue(cleanOption);
	var replacementsValue = parseResult.GetValue(replaceOptions) ?? [];
	var stripValue = parseResult.GetValue(stripOptions) ?? [];
	var customMapsValue = parseResult.GetValue(mapOptions) ?? [];
	var packsPathValue = parseResult.GetValue(packsOptions)!;
	var dotnetVersionValue = parseResult.GetValue(dotnetVersionOptions);
	var logLevelValue = parseResult.GetValue(logLevelOptions);
	var buildZodSchemasValue = parseResult.GetValue(buildZodSchemasOptions);
	var generateApiClientsValue = parseResult.GetValue(generateApiClientsOptions);
	var apiClientsTemplateValue = parseResult.GetValue(apiClientsTemplateOptions)!;
	var casingValue = parseResult.GetValue(casingOptions);

	Log.Instance = new ConsoleLogger(logLevelValue);
	var generator = new Generator(assemblyOptionValue,
								  outputValue,
								  relativeRootValue,
								  cleanValue,
								  replacementsValue,
								  stripValue,
								  customMapsValue,
								  packsPathValue,
								  dotnetVersionValue,
								  buildZodSchemasValue,
								  generateApiClientsValue,
								  apiClientsTemplateValue,
								  casingValue);

	return await generator.Execute(cancellationToken);
});

var parsedResult = rootCommand.Parse(args);
return await parsedResult.InvokeAsync();
