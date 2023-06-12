# TypeContractor

Looks at one or more assemblies containing contracts and translates to TypeScript interfaces and enums

## Goals

1. Take one or more assemblies and reflect to find types matching a list
   of suffixes (by default `Dto`, `Request`, and `Response`). If a type doesn't
   match the suffix, it gets ignored -- unless a type that *is* matched
   has this type as a dependency. In that case, we have to have the type
   even if the suffix doesn't match.

   For example:

   We have a type `GetPaymentListResponse` that gets included from the default
   list of suffixes. That type has a list of `GetPaymentListResponse_Payment`,
   which would be ignored by configuration, but since it is a part of 
   `GetPaymentListResponse`, we convert both.

   `GetPaymentListResponse_Payment` also has a reference to an enum,
   `PaymentState`, that gets ignored by the configuration. But again, since it
   is a dependency, it gets converted.

2. Perform replacements on the names to strip away prefixes

   `Hogia.SalaryService.Common` is unnecessary to have in the output path.
   We can strip the common prefix and get `api/Modules/Absence/AbsenceRequest.ts`
   instead of `api/Hogia/SalaryService/Common/Modules/Absence/AbsenceRequest.ts`.

3. Map custom types to their TypeScript-friendly counterparts if necessary.

   For example, SalaryService has a custom `Money` type that maps down to
   `number`. If we don't configure that manually, it will create the `Money`
   interface, which only contains `amount` as a `number`. That's both
   cumbersome to work with, as well as *wrong*, since the BFF will 
   serialize `Money` as a `number`.


## Setup and configuration

To accomplish the same configuration and results as described under Goals,
create Contractor like this:

```csharp
Contractor.FromDefaultConfiguration(configuration => configuration
            .AddAssembly("Hogia.SalaryService.Common", "Hogia.SalaryService.Common.dll")
            .AddCustomMap("Hogia.SalaryService.Common.Types.Money", DestinationTypes.Number)
            .StripString("Hogia.SalaryService.Common")
            .SetOutputDirectory(Path.Combine(Directory.GetCurrentDirectory(), "api")));
```

## Run manually

Get an instance of `Contractor` and call `contractor.Build();`

## Integrate with ASP.NET Core

The easiest way is to TypeContractor, using `dotnet
tool install --global typecontractor`.  This adds `typecontractor` as an
executable installed on the system and always available.

Run `typecontractor` to get a list of available options.

This tool reflects over the main assembly provided and finds all controllers
(that inherits from `Microsoft.AspNetCore.Mvc.ControllerBase`).  Each
controller is reflected over in turn, and finds all public methods that returns
`ActionResult<T>`. The `ActionResult` is unwrapped and the inner type is added
to a list of candidates.

For each candidate, we apply stripping and replacements and custom mappings and
write everything to the output files.

### Installing locally

Instead of installing the tool globally, you can also add it locally to the
project that is going to use it. This makes it easier to make sure everyone
who wants to run the project have it available.

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
      Command="typecontractor --assembly $(OutputPath)$(AssemblyName).dll --output $(MSBuildThisFileDirectory)\App\src\api --clean --replace OpenHR.Lon.Web.App.src.modules:Lon --replace Infrastructure.Http.Common:Common --strip Hogia" />
  <Message Importance="high" Text="Finished generating API types" Condition="'$(AGENT_ID)' == ''" />
</Target>
```

This will only run in non-CI environments (tested on Azure DevOps). Adjust the
environment variable as needed. You don't want new types to be generated on the
build machine, that should use whatever existed when the developer did their
thing.

It will first:

1. Strip out `Hogia.` from the beginning of namespaces
2. Replace `OpenHR.Lon.Web.App.src.modules` with `Lon`
3. Replace `Infrastructure.Http.Common` with `Common`

by looking at the configured assembly. The resulting files are placed in
`Web\App\src\api`.

## Integration with MSBuild

An alternative set of integrating with ASP.NET Core is to hook into the MSBuild
build system that runs after build. First install the package called
`TypeContractor.MSBuild`.  This package provides a custom MSBuild task that
makes the magic happen.

This task reflects over the main assembly provided and finds all controllers
(that inherits from `Microsoft.AspNetCore.Mvc.ControllerBase`).  Each
controller is reflected over in turn, and finds all public methods that returns
`ActionResult<T>`. The `ActionResult` is unwrapped and the inner type is added
to a list of candidates.

For each candidate, we apply stripping and replacements and custom mappings and
write everything to the output files.

### Configuration

In your `Web.csproj`, install `TypeContractor.MSBuild`, then add a target that
calls the task. Example:

```xml
<Target Name="GenerateTypes" AfterTargets="Build">
		<Message Importance="high" Text="Running in CI, not generating new types" Condition="'$(AGENT_ID)' != ''" />
		<Message Importance="high" Text="Generating API types" Condition="'$(AGENT_ID)' == ''"/>

		<ItemGroup>
			<TypeContractor_StripStrings Include="Hogia" />
			<TypeContractor_Replacements Include="OpenHR.Lon.Web.App.src.modules:Lon" />
			<TypeContractor_Replacements Include="Infrastructure.Http.Common:Common" />
		</ItemGroup>
		<GenerateApiTypes Condition="'$(AGENT_ID)' == ''"
						  Replacements="@(TypeContractor_Replacements)"
						  StripStrings="@(TypeContractor_StripStrings)"
						  TypesOutputPath="$(MSBuildThisFileDirectory)\App\src\api"
						  CleanOutputPath="true"
						  AssemblyPath="$(OutputPath)$(AssemblyName).dll"/>
	</Target>
```

This will only run in non-CI environments (tested on Azure DevOps). Adjust the
environment variable as needed. You don't want new types to be generated on the
build machine, that should use whatever existed when the developer did their
thing.

It will first:

1. Strip out `Hogia.` from the beginning of namespaces
2. Replace `OpenHR.Lon.Web.App.src.modules` with `Lon`
3. Replace `Infrastructure.Http.Common` with `Common`

by looking at the configured assembly. The resulting files are placed in
`Web\App\src\api`.


## Future improvements

* Kebab-case output files and directories
* Better documentation
* Better performance -- if this should run on build, it can't take forever
* Install as a global dotnet tool?
* Improve method for finding AspNetCore framework DLLs

