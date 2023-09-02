# TypeContractor

Looks at one or more assemblies containing contracts and translates to
TypeScript interfaces and enums

## Goals

1. Take one or more assemblies and reflect to find relevant types and their
   dependencies that should get a TypeScript definition published.

2. Perform replacements on the names to strip away prefixes

   `MyCompany.SystemName.Common` is unnecessary to have in the output path. 
   We 
   can strip the common prefix and get `api/Modules/MyModule/SomeRequest.ts`
   instead of 
   `api/MyCompany/SystemName/Common/Modules/MyModule/SomeRequest.ts`.

3. Map custom types to their TypeScript-friendly counterparts if necessary.

   For example, say your system has a custom `Money` type that maps down to
   `number`. If we don't configure that manually, it will create the `Money`
   interface, which only contains `amount` as a `number`. That's both
   cumbersome to work with, as well as *wrong*, since the serialization will
   (most likely) serialize `Money` as a `number`.


## Setup and configuration

To accomplish the same configuration and results as described under Goals,
create Contractor like this:

```csharp
Contractor.FromDefaultConfiguration(configuration => configuration
            .AddAssembly("MyCompany.SystemName.Common", "MyCompany.SystemName.Common.dll")
            .AddCustomMap("MyCompany.SystemName.Common.Types.Money", DestinationTypes.Number)
            .StripString("MyCompany.SystemName.Common")
            .SetOutputDirectory(Path.Combine(Directory.GetCurrentDirectory(), "api")));
```

## Run manually

Get an instance of `Contractor` and call `contractor.Build();`

## Integrate with ASP.NET Core

The easiest way is to TypeContractor, using `dotnet tool install --global
typecontractor`.  This adds `typecontractor` as an executable installed on the
system and always available.

Run `typecontractor` to get a list of available options.

This tool reflects over the main assembly provided and finds all controllers
(that inherits from `Microsoft.AspNetCore.Mvc.ControllerBase`). Each controller
is reflected over in turn, and finds all public methods that returns
`ActionResult<T>`. The `ActionResult<T>` is unwrapped and the inner type `T` is
added to a list of candidates.

Additionally, the public methods returning an `ActionResult<T>` *or* a plain
`ActionResult` will have their parameters analyzed as well. Anything that's not
a builtin type will be added to the list of candidates.

Meaning if you have a method looking like:

```csharp
public async Task<ActionResult> Create([FromBody] CreateObjectDto request, CancellationToken cancellationToken)
{
   ...
}
```

we will add `CreateObjectDto` to the list of candidates.
`System.Threading.CancellationToken` is a builtin type (currently, this means
it is defined inside `System.`) and will be ignored. Same with other basic
types such as `int`, `Guid`, `IEnumerable<T>` and so on.

For each candidate, we apply stripping and replacements and custom mappings and
write everything to the output files.

### Installing locally

Instead of installing the tool globally, you can also add it locally to the
project that is going to use it. This makes it easier to make sure everyone who
wants to run the project have it available.

For the initial setup, run:

`dotnet new tool-manifest`
`dotnet tool install typecontractor`

in your Web-project.

Whenever new users check out the repository, they can run `dotnet tool restore`
and get everything you need installed.

### Running automatically

In your `Web.csproj` add a target that calls the tool after build. Example:

```xml
<Target Name="GenerateTypes" AfterTargets="Build">
  <Message Importance="high" Text="Running in CI, not generating new types" Condition="'$(AGENT_ID)' != ''" />
  <Message Importance="high" Text="Generating API types" Condition="'$(AGENT_ID)' == ''" />
  <Exec 
      Condition="'$(AGENT_ID)' == ''"
      ContinueOnError="true"
      Command="typecontractor --assembly $(OutputPath)$(AssemblyName).dll --output $(MSBuildThisFileDirectory)\App\src\api --clean smart --replace My.Web.App.src.modules:App --replace Infrastructure.Common:Common --strip MyCompany" />
  <Message Importance="high" Text="Finished generating API types" Condition="'$(AGENT_ID)' == ''" />
</Target>
```

This will only run in non-CI environments (tested on Azure DevOps). Adjust the
environment variable as needed. You don't want new types to be generated on the
build machine, that should use whatever existed when the developer did their
thing.

It will first:

1. Strip out `MyCompany.` from the beginning of namespaces
2. Replace `My.Web.App.src.modules` with `App`
3. Replace `Infrastructure.Common` with `Common`

by looking at the configured assembly. The resulting files are placed in
`Web\App\src\api`.

When running with `--clean smart`, which is the default, it first generates the
updated or newly created files. After that, it looks in the output directory
and removes every file and directory that are no longer needed.

Other options are:

* `none` -- which as the name suggests, does no cleanup at all.
  That part is left as an exercise to the user.
* `remove` -- which removes the entire output directory before
  starting the file generation. This is probably the quickest, but some tools
  that are watching for changes does not always react so well to having files
  suddenly disappear and reappear.

## Future improvements

* Kebab-case output files and directories
* Better documentation
* Better performance -- if this should run on build, it can't take forever
* Possible to add types to exclude?
* Improve method for finding AspNetCore framework DLLs
  * Possible to provide a manual path, so not a priority
* Work with Hot Reload?
