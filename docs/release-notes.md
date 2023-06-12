#[0.3.1.0](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/b5416a0660889b0602bd5a9e64236a633a84b46e) - 2023-06-12

####Bug fixes or code changes
* fix(lib): Add more null checks [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/625a7b76936db33713b893454ba9984db24b1429)

####Other changes
* chore(lib): Add PackageId to TypeContractor main library [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/fd2a731e4e202ae1cf31705c86ad31fc9468daf5)

#[0.3.0.0](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/362ae5534edc6920c7629458d8f3f2f353511fd6) - 2023-06-06

####New functionality
* feat(lib): Add support for simple ValueTuple types [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/ac80627009ecff40928dfb86278809590c0478ef)
* feat(lib): Add support for nested classes [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/4a99e89bec2adca66408fa3cbdc1b68afd6b4772)

####Other changes
* chore(lib): Add better debugging information when paths fail [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/1a49207a1d682ea46d6ca5ffded94e14b66f5611)

#[0.2.0.0](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/ad4a280b03135e2dc2a08ace58f7198ae64814dd) - 2023-06-06

####New functionality
* feat(lib): Add support for more complex dictionaries [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/73b64363dd901fc9420c9d7d8d0ed1bff3c5333b)
* feat(lib): Add support for simple dictionaries [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/a19644d4d7f0e25fd413517207eb578479b68205)

####Bug fixes or code changes
* fix(lib): Fix multiple enumeration warning [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/f7d2a93ae56be297a292b1cfa6dfe354a42dfea7)
* refactor(lib): Minor cleanup in type checking [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/d377916681eda4f8f71c7bc2ba546598865fcf00)
* refactor(lib): Extract main program into TypeContractor.Example [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/9346a452bce2d8e03486bfd0a4ce05fa47878a1e)

####Other changes
* test(lib): Add some tests for TypeScriptConverter [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/8e53ae41e0e4736cd9fbcd31f27e40af2258c666)

#[0.1.0](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/0fc997d6238b5b6ce317ebcc4899c7bd82bb17a8) - 2023-05-30

####New functionality
* feat(lib): Use MetadataLoadContext instead of Assembly directly [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/226c37492eebc986eb47d5506934fab024684a3a)
* feat(lib): Allow adding multiple assemblies at once [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/e50f6601e4041e2838775a7d865e712e9b3fef72)
* feat(lib): Allow adding explicit list of types [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/7abe80418cb7a7034e2d832ed9cc07217ea9bc86)
* feat(lib): Add a fluent configuration interface [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/35460fb0b7ff5816ebeac2b38a019c1ad1708e75)

####Bug fixes or code changes
* refactor(lib): Use file-scoped namespaces [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/fd76153e600fb09c51f3a94e3cc1a1754f147cb9)
* fix(lib): Handle finding generic types when inheriting [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/2e29ec936fb92c9934f82b2b0154dff4c3837539)
* refactor(lib): Build as library, not executable [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/07cb86b172bfb2f2042e5b8077b0d75362c8de88)
* refactor(lib): Rename private configuration variable [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/8083a0dfd01fbe9d1715ca74094837adb8709d9a)
* refactor(lib): Allow string-based mapping of types [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/6bb00dfe76cea075434cfae9c923fa3f731aaf8d)
* refactor(lib): Rename Configuration to avoid conflict [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/173513bb007e930388146847f175097f41e421fa)
* refactor(lib): Move actual work out of Program [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/a76ac6badc5a05b1551b289d5bb7a87d2825f724)
* fix(lib): Use folder name to find relative paths [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/e59d538474da07b1cf7d013746af9e2b6fa1d506)
* refactor(lib): Make sure we remove double-slashes in imports [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/aa01b9f37498cdc951e3f59ec044eaf65990f31f)
* fix(lib): Fix issue with relative paths [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/b64dceb29b05fb952b5b09bf3a512bdb5e5bce28)
* refactor(lib): Add example project to convert [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/39c1a95e8ba0a68a73b083a08ca5e11dedc4564b)
* refactor(lib): Avoid writing some files twice [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/7565acae36076f5061ab9d81529a96b7d996e74c)

####Other changes
* chore(lib): Check argument for null in `AddAssemblies` [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/fc2b78ff19d41101ab85e70211d19f4e99faba42)
* docs(lib): Start on some documentation [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/0317a4328fc49aceb8a7989c3030544bbb26e150)
* chore(lib): Add more complex examples to example classlib [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/926424e5e9494e7ca534c3b60f66efa3a44b2dc6)
* chore(lib): Clean up usings [Per Christian B. Viken](https://dev.azure.com/Hogia/HRS/_git/HACK_TypeContractor/commit/933615f6cfe23d206d7d99e8bda109b7b9d12aa9)

