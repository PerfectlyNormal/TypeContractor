# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [unreleased]

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


[unreleased]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.7.0...HEAD
[0.7.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.6.0...v0.7.0
[0.6.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.5.1...v0.6.0
[0.5.1]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.5.0...v0.5.1
[0.5.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.3.1...v0.4.0
[0.3.1]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.3.0...v0.3.1
[0.3.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/PerfectlyNormal/TypeContractor/compare/v0.0.1...v0.1.0
