#[0.5.1.0](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/03f81f47322456fafdd192d08d7a2de65df65ab0) - 2023-07-28

####Bug fixes or code changes
* fix(lib): Can't use `typeof` when comparing types [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/c1211af410d2e54a2f88aa319669c1c65815fd3c)

####Other changes
* chore(lib): Improve debugging when importing fails [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/c654513b6367f61cfbde3993ece6efd77c88d40b)

#[0.5.0.0](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/0e62d568a22d0b99741ee54cda4427c5696d614e) - 2023-07-27

####New functionality
* feat(lib): Add mapping from `dynamic` to `any` [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/d5023a628557c168a6347570af7e1c40f7cdf667)

#[0.4.0.0](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/533faebace82f8b698c695f26a89cc1319cc5f4c) - 2023-06-24
#####[Lön: Generera TypeScript-typer automatiskt](https://dev.azure.com/Hogia/HRS/_workitems/edit/245172)

####New functionality
* feat(lib, tool): Find parameters for endpoints [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/f4d82ab03931fd3ebd7c059e1e9ba5b7d805dcbc)

####Bug fixes or code changes
* refactor(lib, tool): Move type checks to library [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/bc4d0e96649d6efea837c670761c0c71a18e6524)

#[0.3.1.0](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/b5416a0660889b0602bd5a9e64236a633a84b46e) - 2023-06-12

####Bug fixes or code changes
* fix(lib): Add more null checks [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/625a7b76936db33713b893454ba9984db24b1429)

####Other changes
* chore(lib): Add PackageId to TypeContractor main library [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/fd2a731e4e202ae1cf31705c86ad31fc9468daf5)

#[0.3.0.0](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/f6ead2f51e8497c69e2ed8b678508bdef2f2297a) - 2023-06-06

####New functionality
* feat(lib): Add support for simple ValueTuple types [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/ac80627009ecff40928dfb86278809590c0478ef)
* feat(lib): Add support for nested classes [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/4a99e89bec2adca66408fa3cbdc1b68afd6b4772)

####Bug fixes or code changes
* refactor(msbuild): Reuse type check from main library [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/4e422da77f08af845849fabfdfa6eb9a1f27b03b)

####Other changes
* chore(lib): Add better debugging information when paths fail [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/1a49207a1d682ea46d6ca5ffded94e14b66f5611)

#[0.2.0.0](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/0da0a9033a6a84865329b529cdb4276ac8651d12) - 2023-06-06

####New functionality
* feat(lib): Add support for more complex dictionaries [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/73b64363dd901fc9420c9d7d8d0ed1bff3c5333b)
* feat(lib): Add support for simple dictionaries [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/a19644d4d7f0e25fd413517207eb578479b68205)

####Bug fixes or code changes
* fix(lib): Fix multiple enumeration warning [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/f7d2a93ae56be297a292b1cfa6dfe354a42dfea7)
* refactor(lib): Minor cleanup in type checking [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/d377916681eda4f8f71c7bc2ba546598865fcf00)
* refactor(lib): Extract main program into TypeContractor.Example [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/9346a452bce2d8e03486bfd0a4ce05fa47878a1e)

####Other changes
* test(lib): Add some tests for TypeScriptConverter [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/8e53ae41e0e4736cd9fbcd31f27e40af2258c666)

#[0.1.0](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/ab26ff08868962c263b1563e610631f0c2f04db5) - 2023-05-30

####New functionality
* feat(msbuild): Add MSBuild package [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/01a96ff9105e986eeabbc788e59c8c6de97af0ab)

####Bug fixes or code changes
* fix(msbuild): Include vendored MetadataLoadContext.dll [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/c2c35eaa13f1b0e24e90dca1eb8372abe30d19be)

####Other changes
* chore(msbuild): Remove explicit nuget version [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/bbebe1419b4c3f6b0a889baa2d7d041be1c55bca)

