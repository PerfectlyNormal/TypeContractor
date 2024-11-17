# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [unreleased]

## [0.14.0] - 2024-11-17

### Added

- Handle optional segments in routes for API clients (#77)
- Added configuration file support (#90)

### Fixed

- Don't generate types for parameters with an `[FromServices]` annotation (#95)
- Add default `any`-mapping for `IActionResult` as a return type (#98)
- Log an error if we're unable to match a route parameter when generating API clients (#86)
- Revert the "response files" fix in [0.13.1], since it broke default command
  line handling, including the --help command

### Changed

- Standardize on encoding generated files as UTF-8 without BOM
- Write a final newline in the files
- Standardize on tabs for indentation
- Fix some editorconfig settings

## [0.13.1] - 2024-11-11

### Added

- Add basic template for React with Axios

### Fixed

- Ignore response files when parsing command line, so "@/Api" can be accepted as a valid relative root

### Changed

- Update System.Reflection.MetadataLoadContext to v8.0.1
- Update xunit to v2.9.2
- Update xunit.analyzers to v1.17.0
- Update FluentAssertions to v6.12.2

## [0.13.0] - 2024-11-09

### Added

- Generate API clients using Handlebars templates, with an option to provide custom templates (#75)

### Fixed

- Strip trailing slash in API clients (#87)

## [0.12.7] - 2024-10-10

### Fixed

- Only import Zod in generated API clients if we actually want it
- Set nullable enums as `.nullable()`, not `.optional()` in Zod schema (#82)

### Changed

- Standardize on single quotes for imports

## [0.12.6] - 2024-09-18

### Fixed

- Don't blindly append controller prefix to API client URLs (#76)

## [0.12.5] - 2024-09-18

### Fixed

- Slice of the initial slash in API client URLs.
- Fix formatting for arrays as query strings

## [0.12.4] - 2024-09-18

### Fixed

- Use the correct variable reference when unpacking query parameters (Really fix #68)

## [0.12.3] - 2024-09-17

### Fixed

- Handle nullable parameters to API clients better. Only send query parameters if we have a value
- Unpack non-builtin query parameters automatically (#68)

### Changed

- Update xunit.analyzers from v1.15.0 to v1.16.0
- Update FluentAssertions from v6.12.0 to v6.12.1
- Update Microsoft.NET.Test.Sdk from v17.11.0 to 17.11.1

## [0.12.2] - 2024-09-16

### Fixed

- Handle enumerable parameters in API clients

## [0.12.1] - 2024-09-13

### Fixed

- Improve detection of FromRoute/FromQuery parameters in API clients
- Make sure query parameters are stringified for API clients
- Fix import paths for nested classes for API clients
- Improve handling of builtin return types for API clients

## [0.12.0] - 2024-09-13

### Added

- Experimental creation of [Zod schemas](https://zod.dev/)
- Automatic generation of API clients

### Changed

- Update xunit and friends
- Update Microsoft.NET.Test.Sdk to 17.11.0

## [0.11.0] - 2024-06-21

### Added

- Include `@deprecated` JSDoc if property is marked as `[Obsolete]` (#45)

### Fixed

- Handle nullability for more types, including `string` and records (#51)

### Changed

- Update xunit and friends
- Update Microsoft.NET.Test.Sdk to 17.10.0

## [0.10.0] - 2024-04-21

### Added

- Add default map from `System.DateOnly` and `System.TimeOnly` to `string`

### Changed

- Update xunit to 2.7.1
- Update xunit.analyzers to 1.12.0
- Update xunit.runner.visualstudio to 2.5.8
- Update coverlet.collector to 6.0.2
- Update Microsoft.NET.Test.Sdk to 17.9.0

## [0.9.2] - 2024-01-28

### Fixed

- Work around failing to load `System.Text.Json`, which it claims we're
  doing twice in some cases. (#28)

### Changed

- Add global singleton `Log.Instance` to simplify logging in helpers
- Minor code fixes suggested by Visual Studio
- Use a source-generated regex for creating TypeScript names
- Update xunit to v2.6.6

## [0.9.1] - 2024-01-07

### Added

- Add default map from `System.Object` to `any`

### Fixed

- Fix writing dictionaries with custom types as values wrapped inside lists.
  E.g. converting `Dictionary<Guid, IEnumerable<FormulaDto>>` to
  `{ [key: string]: FormulaDto[] }`
- Fix writing dictionaries wrapped inside other dictionaries, e.g. making sure
  `Dictionary<Guid, Dictionary<string, IEnumerable<FormulaDto>>>` correctly
  translates to `{ [key: string]: { [key: string]: FormulaDto[] } }`.

## [0.9.0] - 2024-01-05

### Added

- Tool and library built using .NET8 (#14)
- Add `--dotnet-version` option to set the dotnet version used for finding
  framework DLLs. Defaults to 8.

### Changed

- Update xunit to v2.6.5

## [0.8.1] - 2023-12-24

### Fixed

- Add default maps from `System.Double` to `number` and `System.TimeSpan` to `string` (#19)

### Changed

- Improve error logging if a type fails to convert (#20)
- Update xunit to v2.6.4
- Update xunit.runner.visualstudio to v2.5.6
- Update xunit.analyzers to v1.8.0

## [0.8.0] - 2023-12-05

### Added

- Tool exits with code 0 when everything is okay and 1 when something has gone wrong

### Fixed

- Log an error and continue when we fail to convert a file

### Changed

- Enable Dependabot version updates
- Update xunit to v2.6.2
- Update xunit.runner.visualstudio to v2.5.4
- Update xunit.analyzers to v1.6.0
- Fix compiler warnings

## [0.7.0] - 2023-09-17

### Added

- Add support for `readonly` output properties (#1)

### Fixed

- Properly handle `IEnumerable<T>` as values in a Dictionary (#2)

## [0.6.0] - 2023-09-02

### Added

- Add smart cleanup to help file-watching tools not freak out when
  everything changes at once.
- Add a `ILog` inside `TypeContractor` to simplify adding debug information

### Removed

- Remove the MSBuild-based tool, to focus on the dotnet-tool instead.

## [0.5.1] - 2023-07-28

### Fixed

- Replace `typeof` comparison with a name-based approach

### Changed

- Add better debug output when we can't find a type to import

## [0.5.0] - 2023-07-28

### Added

- Add default mapping from `dynamic` to `any`

## [0.4.0] - 2023-06-24

### Added

- Add configurability to the dotnet-tool
- Automatically find parameters for endpoints

### Changed

- Move code from tool into library for better sharing

## [0.3.1] - 2023-06-12

### Added

- Add the dotnet-tool

### Fixed

- Add more null checks

### Changed

- Add PackageId to nuget libraries

## [0.3.0] - 2023-06-06

### Added

- Add support for mapping nested classes
- Add support for mapping (simple) `ValueTuple` types

### Changed

- Add better debug when import paths fail to find a common ancestor

## [0.2.0] - 2023-06-06

### Added

- Add support for mapping `Dictionary<TKey, TValue>` to TypeScript

### Fixed

- Fix compiler warnings

### Changed

- Add more unit tests
- Minor code cleanup

## [0.1.0] - 2023-05-30

### Added

- Initial release

[unreleased]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.14.0...HEAD
[0.14.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.14.0...v0.13.1
[0.13.1]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.13.1...v0.13.1
[0.13.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.13.0...v0.12.7
[0.12.7]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.12.7...v0.12.6
[0.12.6]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.12.6...v0.12.5
[0.12.5]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.12.5...v0.12.4
[0.12.4]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.12.4...v0.12.3
[0.12.3]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.12.3...v0.12.2
[0.12.2]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.12.2...v0.12.1
[0.12.1]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.12.1...v0.12.0
[0.12.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.12.0...v0.11.0
[0.11.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.11.0...v0.10.0
[0.10.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.10.0...v0.9.2
[0.9.2]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.9.2...v0.9.1
[0.9.1]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.9.1...v0.9.0
[0.9.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.9.0...v0.8.1
[0.8.1]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.8.0...v0.8.1
[0.8.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.7.0...v0.8.0
[0.7.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.6.0...v0.7.0
[0.6.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.5.1...v0.6.0
[0.5.1]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.5.0...v0.5.1
[0.5.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.3.1...v0.4.0
[0.3.1]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.3.0...v0.3.1
[0.3.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.0.1...v0.1.0
