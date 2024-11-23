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

4. Help validate responses from the API

   Regular `response.json()` returns `any`, which means that any return type
   we specify ourselves are just wishful thinking. It might match, it might
   differ a bit, or it can differ a lot. Using [Zod](https://zod.dev/), we
   generate schemas automatically that can be used for validating the
   response and _knowing_ that we get the correct data back.

5. Automatically generate API clients based on the controller endpoints.

   Why bother creating a TypeScript API client where you have to manually
   keep routes, parameters and everything in sync? We have the data, we have
   the types, we have the schemas. Let's just make everything work together,
   and let TypeContractor handle keeping those pesky API changes in sync.


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

## Configuration file

It's possible to create a configuration file and provide all the same options
via a file instead of command line arguments.

Create a file called `typecontractor.config` in the same or a parent directory
from where you run TypeContractor from. We use [dotnet-config] for parsing the
files (written in [TOML]), so it's possible to inherit from parent directories,
a user-wide configuration or a system-wide configuration. Or have a
repository-specific configuration that can be overridden inside the repo with
`typecontractor.config.user`. You can edit the file manually or install the 
dotnet-config tool for viewing and changing configuration similar to how
`git config` works.

### Example configuration file

```toml
[typecontractor]
    # backslashes must be escaped
    assembly = "bin\\Debug\\net8.0\\MyCompany.SystemName.dll"
    output = "api"
    strip = "MyCompany"
    replace = "MyCompany.Common:CommonFiles" # Options can repeat
    replace = "ThirdParty.Http:Http"
    root = "~/api"
    generate-api-clients = true
    build-zod-schemas = true
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

## Integration with Zod

An experimental option to generate [Zod schemas](https://zod.dev/) exists
behind the `--build-zod-schemas` flag. This causes each generated TypeScript
file to also have a `<TypeName>Schema` generated that can be integrated with
Zod. Currently no support for validations, but that might come in a future
update.

Example:

```csharp
public class PaymentsPerYearResponse
{
    public IEnumerable<int> Years { get; set; } = [2023, 2024];
    public Dictionary<int, int> PaymentsPerYear { get; set; } = new Dictionary<int, int>
    {
        { 2024, 4 },
        { 2023, 12 }
    };
}
```

generates

```typescript
import { z } from 'zod';

export interface PaymentsPerYearResponse {
  years: number[];
  paymentsPerYear: { [key: number]: number };
}

export const PaymentsPerYearResponseSchema = z.object({
  years: z.array(z.number()),
  paymentsPerYear: z.record(z.string(), z.number()), // JavaScript is very stringy (https://zod.dev/?id=records)
});
```

and can be integrated using something similar to

```typescript
const response = await this.http.fetch('api/paymentsPerYear', { signal: cancellationToken });
const input = await response.json();
return PaymentsPerYearResponseSchema.parse(input);
```

which will throw a `ZodError` if `input` fails to parse against the schema.
Otherwise it returns a cleaned up version of `input`.

## Automatic API client generation

By running TypeContractor with the `--generate-api-clients` flag, every
controller found will have an automatic client generated for each of the
discovered endpoints.

Should be combined with the `--root` flag for generating nice imports.
For example `--root "~/api"` if you are writing files to `api` and have
`~` defined as an alias in your bundler.

If Zod integration is enabled, the return type for each API call will
be automatically validated against the schema. If so, it expects
the `Response` prototype to be extended with a `parseJson` method.
An example implementation can be found in `tools/response.ts`.

To provide a custom template instead of using the built-in Aurelia one,
provide `--api-client-template` with the path to a Handlebars template
that does what you want. The available data model can be found in
`TypeContractor/Templates/ApiClientTemplateDto.cs` and an example
template is `TypeContractor/Templates/aurelia.hbs`.

Since the name is just the controller name with `Controller` replaced
with `Client`, collisions between controllers with the same name but
different namespaces are possible. If this happens, the first
controller found gets to keep the original name and TypeContractor
will prefix the colliding client with the last part of its namespace.
So for example:

`ExampleApp.Controllers.DataController` turns into `DataClient`,
while `ExampleApp.Controllers.Subsystem.DataController` collides and
gets turned into `SubsystemDataClient`.

## Further customization using Annotations

To further customize the output of TypeContractor, you can install
the optional package `TypeContractor.Annotations` and start annotating
your controllers.

Available annotations:

* `TypeContractorIgnore`:
  If you have a controller that doesn't need a client
  generated, you can annotate that controller using `TypeContractorIgnore`
  and it will be automatically skipped.
* `TypeContractorClient`:
  If you have a badly named controller that you can't rename,
  you want something custom, or just don't like the default naming
  scheme, you can apply this attribute to select a brand new name.

## Future improvements

* Kebab-case output files and directories
* Better documentation
* Better performance -- if this should run on build, it can't take forever
* Possible to add types to exclude?
* Improve method for finding AspNetCore framework DLLs
  * Possible to provide a manual path, so not a priority
* Work with Hot Reload?

[dotnet-config]: https://dotnetconfig.github.io/dotnet-config/index.html
[TOML]: https://toml.io/en/
