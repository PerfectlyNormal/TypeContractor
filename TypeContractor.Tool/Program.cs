using System.CommandLine;
using TypeContractor.Tool;

var rootCommand = new RootCommand("Tool for generating TypeScript definitions from C# code");
var assemblyOption = new Option<string>("--assembly", "Path to the assembly to start with. Will be relative to the current directory");
var outputOption = new Option<string>("--output", "Output path to write to. Will be relative to the current directory");
var cleanOption = new Option<bool>("--clean", () => false, "If true, remove every existing file before generating new data. Danger!");
var replaceOptions = new Option<string[]>("--replace", "Provide one replacement in the form '<search>:<replace>'. Can be repeated");
var stripOptions = new Option<string[]>("--strip", "Provide a prefix to strip out of types. Can be repeated");
var mapOptions = new Option<string[]>("--custom-map", "Provide a custom type map in the form '<from>:<to>'. Can be repeated");
var packsOptions = new Option<string>("--packs-path", () => @"C:\Program Files\dotnet\packs\", "Path where dotnet is installed and reference assemblies can be found.");
var logLevelOptions = new Option<LogLevel>("--log-level", () => LogLevel.Info);
assemblyOption.IsRequired = true;
outputOption.IsRequired = true;

rootCommand.AddOption(assemblyOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(cleanOption);
rootCommand.AddOption(replaceOptions);
rootCommand.AddOption(stripOptions);
rootCommand.AddOption(mapOptions);
rootCommand.AddOption(packsOptions);
rootCommand.AddOption(logLevelOptions);

rootCommand.SetHandler(async (string assemblyOption, string output, bool clean, string[] replacements, string[] strip, string[] customMaps, string packsPath, LogLevel logLevel) =>
{
    Log.SetLevel(logLevel);
    var generator = new Generator(assemblyOption, output, clean, replacements, strip, customMaps, packsPath);
    await generator.Execute();
}, assemblyOption, outputOption, cleanOption, replaceOptions, stripOptions, mapOptions, packsOptions, logLevelOptions);

await rootCommand.InvokeAsync(args);
