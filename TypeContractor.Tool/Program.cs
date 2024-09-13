using System.CommandLine;
using TypeContractor.Logger;
using TypeContractor.Tool;

var rootCommand = new RootCommand("Tool for generating TypeScript definitions from C# code");
var assemblyOption = new Option<string>("--assembly", "Path to the assembly to start with. Will be relative to the current directory");
var outputOption = new Option<string>("--output", "Output path to write to. Will be relative to the current directory");
var relativeRootOption = new Option<string>("--root", "Relative root for generating cleaner imports. For example '~/api'");
var cleanOption = new Option<CleanMethod>("--clean", () => CleanMethod.Smart, "Choose how to clean up no longer relevant type files in output directory. Danger!");
var replaceOptions = new Option<string[]>("--replace", "Provide one replacement in the form '<search>:<replace>'. Can be repeated");
var stripOptions = new Option<string[]>("--strip", "Provide a prefix to strip out of types. Can be repeated");
var mapOptions = new Option<string[]>("--custom-map", "Provide a custom type map in the form '<from>:<to>'. Can be repeated");
var packsOptions = new Option<string>("--packs-path", () => @"C:\Program Files\dotnet\packs\", "Path where dotnet is installed and reference assemblies can be found.");
var dotnetVersionOptions = new Option<int>("--dotnet-version", () => 8, "Major version of dotnet to look for");
var logLevelOptions = new Option<LogLevel>("--log-level", () => LogLevel.Info);
var buildZodSchemasOptions = new Option<bool>("--build-zod-schemas", () => false, "Enable experimental support for Zod schemas alongside generated types.");
assemblyOption.IsRequired = true;
outputOption.IsRequired = true;

rootCommand.AddOption(assemblyOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(relativeRootOption);
rootCommand.AddOption(cleanOption);
rootCommand.AddOption(replaceOptions);
rootCommand.AddOption(stripOptions);
rootCommand.AddOption(mapOptions);
rootCommand.AddOption(packsOptions);
rootCommand.AddOption(dotnetVersionOptions);
rootCommand.AddOption(logLevelOptions);
rootCommand.AddOption(buildZodSchemasOptions);

rootCommand.SetHandler(async (context) =>
{
    var assemblyOptionValue = context.ParseResult.GetValueForOption(assemblyOption)!;
    var outputValue = context.ParseResult.GetValueForOption(outputOption)!;
    var relativeRootValue = context.ParseResult.GetValueForOption(relativeRootOption);
    var cleanValue = context.ParseResult.GetValueForOption(cleanOption);
    var replacementsValue = context.ParseResult.GetValueForOption(replaceOptions) ?? [];
    var stripValue = context.ParseResult.GetValueForOption(stripOptions) ?? [];
    var customMapsValue = context.ParseResult.GetValueForOption(mapOptions) ?? [];
    var packsPathValue = context.ParseResult.GetValueForOption(packsOptions)!;
    var dotnetVersionValue = context.ParseResult.GetValueForOption(dotnetVersionOptions);
    var logLevelValue = context.ParseResult.GetValueForOption(logLevelOptions);
    var buildZodSchemasValue = context.ParseResult.GetValueForOption(buildZodSchemasOptions);

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
                                  buildZodSchemasValue);

    context.ExitCode = await generator.Execute();
});

return await rootCommand.InvokeAsync(args);
