using HandlebarsDotNet;
using System.Reflection;
using System.Runtime.InteropServices;
using TypeContractor.Helpers;
using TypeContractor.Output;
using TypeContractor.TypeScript;

namespace TypeContractor.Tests.TypeScript;

public sealed class ApiClientWriterTests : IDisposable
{
	private readonly DirectoryInfo _outputDirectory;
	private readonly TypeContractorConfiguration _configuration;
	private readonly TypeScriptConverter _converter;
	private readonly HandlebarsTemplate<object, object> _templateFn;

	public ApiClientWriter Sut { get; }

	public ApiClientWriterTests()
	{
		var assembly = typeof(ApiClientWriterTests).Assembly;
		_outputDirectory = Directory.CreateTempSubdirectory();
		_configuration = TypeContractorConfiguration
			.WithDefaultConfiguration()
			.AddAssembly(assembly.FullName!, assembly.Location)
			.SetOutputDirectory(_outputDirectory.FullName);
		_converter = new TypeScriptConverter(_configuration, BuildMetadataLoadContext());
		Sut = new ApiClientWriter(_configuration.OutputPath, "~");

		var embed = typeof(ApiClientWriter).Assembly.GetManifestResourceStream("TypeContractor.Templates.aurelia.hbs");
		using var sr = new StreamReader(embed!);
		var template = sr.ReadToEnd();
		_templateFn = Handlebars.Compile(template);
	}

	[Fact]
	public void Can_Write_Basic_Client()
	{
		// Arrange
		var apiClient = new ApiClient("TestClient", "TestController", "test", null);
		apiClient.AddEndpoint(new ApiClientEndpoint("getLatestId", "latest", EndpointMethod.GET, typeof(Guid), typeof(Guid), false, [], null));

		// Act
		var result = Sut.Write(apiClient, [], _converter, false, _templateFn, Casing.Pascal);

		// Assert
		var file = File.ReadAllText(result).Trim();
		file.Should()
			.NotBeEmpty()
			.And.NotContain("import { z } from 'zod';")
			.And.Contain("export class TestClient {")
			.And.Contain("public async getLatestId(cancellationToken: AbortSignal = null): Promise<string> {")
			.And.Contain("const url = new URL('test/latest', window.location.origin);")
			.And.Contain("const response = await this.http.get(`${url.pathname}${url.search}`.slice(1), { signal: cancellationToken });")
			.And.Contain("return await response.json();");
	}

	[Fact]
	public void Strips_Trailing_Slash()
	{
		// Arrange
		var apiClient = new ApiClient("TestClient", "TestController", "test", null);
		apiClient.AddEndpoint(new ApiClientEndpoint("getLatestId", "", EndpointMethod.GET, typeof(Guid), typeof(Guid), false, [], null));

		// Act
		var result = Sut.Write(apiClient, [], _converter, false, _templateFn, Casing.Pascal);

		// Assert
		var file = File.ReadAllText(result).Trim();
		file.Should()
			.NotBeEmpty()
			.And.NotContain("import { z } from 'zod';")
			.And.Contain("export class TestClient {")
			.And.Contain("public async getLatestId(cancellationToken: AbortSignal = null): Promise<string> {")
			.And.Contain("const url = new URL('test', window.location.origin);")
			.And.Contain("const response = await this.http.get(`${url.pathname}${url.search}`.slice(1), { signal: cancellationToken });")
			.And.Contain("return await response.json();");
	}

	[Fact]
	public void Can_Write_Basic_Client_With_Schema()
	{
		// Arrange
		var apiClient = new ApiClient("TestClient", "TestController", "test", null);
		apiClient.AddEndpoint(new ApiClientEndpoint("getLatestId", "latest", EndpointMethod.GET, null, typeof(Guid), false, [], null));

		// Act
		var result = Sut.Write(apiClient, [], _converter, true, _templateFn, Casing.Pascal);

		// Assert
		var file = File.ReadAllText(result).Trim();
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("export class TestClient {")
			.And.Contain("public async getLatestId(cancellationToken: AbortSignal = null): Promise<string> {")
			.And.Contain("const url = new URL('test/latest', window.location.origin);")
			.And.Contain("const response = await this.http.get(`${url.pathname}${url.search}`.slice(1), { signal: cancellationToken });")
			.And.Contain("return await response.parseJson(z.string());");
	}

	[Fact]
	public void Writes_Post_Without_Body()
	{
		// Arrange
		var apiClient = new ApiClient("TestClient", "TestController", "test", null);
		apiClient.AddEndpoint(new ApiClientEndpoint("getLatestId", "latest", EndpointMethod.POST, null, typeof(Guid), false, [], null));

		// Act
		var result = Sut.Write(apiClient, [], _converter, true, _templateFn, Casing.Pascal);

		// Assert
		var file = File.ReadAllText(result).Trim();
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("export class TestClient {")
			.And.Contain("public async getLatestId(cancellationToken: AbortSignal = null): Promise<string> {")
			.And.Contain("const url = new URL('test/latest', window.location.origin);")
			.And.Contain("const response = await this.http.post(`${url.pathname}${url.search}`.slice(1), null, { signal: cancellationToken });")
			.And.Contain("return await response.parseJson(z.string());");
	}

	[Fact]
	public void Writes_Post_With_Body()
	{
		// Arrange
		var apiClient = new ApiClient("TestClient", "TestController", "test", null);
		apiClient.AddEndpoint(new ApiClientEndpoint("getLatestId", "latest", EndpointMethod.POST, null, typeof(Guid), false, [new EndpointParameter("year", typeof(int), null, false, true, false, false, false, false, false, false)], null));

		// Act
		var result = Sut.Write(apiClient, [], _converter, true, _templateFn, Casing.Pascal);

		// Assert
		var file = File.ReadAllText(result).Trim();
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("export class TestClient {")
			.And.Contain("public async getLatestId(year: number, cancellationToken: AbortSignal = null): Promise<string> {")
			.And.Contain("const url = new URL('test/latest', window.location.origin);")
			.And.Contain("const response = await this.http.post(`${url.pathname}${url.search}`.slice(1), json(year), { signal: cancellationToken });")
			.And.Contain("return await response.parseJson(z.string());");
	}

	[Fact]
	public void Writes_Get_With_Query()
	{
		// Arrange
		var apiClient = new ApiClient("TestClient", "TestController", "test", null);
		apiClient.AddEndpoint(new ApiClientEndpoint("getLatestId", "latest", EndpointMethod.GET, null, typeof(Guid), false, [new EndpointParameter("year", typeof(int), null, false, false, false, true, false, false, false, false)], null));

		// Act
		var result = Sut.Write(apiClient, [], _converter, true, _templateFn, Casing.Pascal);

		// Assert
		var file = File.ReadAllText(result).Trim();
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("export class TestClient {")
			.And.Contain("public async getLatestId(year: number, cancellationToken: AbortSignal = null): Promise<string> {")
			.And.Contain("const url = new URL('test/latest', window.location.origin);")
			.And.Contain("url.searchParams.append('year', year.toString());");
	}

	[Fact]
	public void Writes_Get_With_Route()
	{
		// Arrange
		var apiClient = new ApiClient("TestClient", "TestController", "test", null);
		apiClient.AddEndpoint(new ApiClientEndpoint("getLatestId", "latest/{year}", EndpointMethod.GET, null, typeof(Guid), false, [new EndpointParameter("year", typeof(int), null, false, false, true, false, false, false, false, false)], null));

		// Act
		var result = Sut.Write(apiClient, [], _converter, true, _templateFn, Casing.Pascal);

		// Assert
		var file = File.ReadAllText(result).Trim();
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("export class TestClient {")
			.And.Contain("public async getLatestId(year: number, cancellationToken: AbortSignal = null): Promise<string> {")
			.And.Contain("const url = new URL(`test/latest/${year}`, window.location.origin);")
			.And.NotContain("url.searchParams.append(");
	}

	[Fact]
	public void Handles_Get_With_Route_And_Query_Parameters()
	{
		// Arrange
		var apiClient = new ApiClient("TestClient", "TestController", "test", null);
		apiClient.AddEndpoint(new ApiClientEndpoint("getLatestId", "latest/{year}", EndpointMethod.GET, null, typeof(Guid), false,
				[new EndpointParameter("year", typeof(int), null, false, false, true, false, false, false, false, false),
				 new EndpointParameter("reverse", typeof(bool?), null, false, false, false, true, false, false, false, true)], null));

		// Act
		var result = Sut.Write(apiClient, [], _converter, true, _templateFn, Casing.Pascal);

		// Assert
		var file = File.ReadAllText(result).Trim();
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("export class TestClient {")
			.And.Contain("public async getLatestId(year: number, reverse: boolean | undefined, cancellationToken: AbortSignal = null): Promise<string> {")
			.And.Contain("const url = new URL(`test/latest/${year}`, window.location.origin);")
			.And.Contain("if (!!reverse)")
			.And.Contain("url.searchParams.append('reverse', reverse.toString());");
	}

	[Fact]
	public void Handles_Enum_As_Query_Parameter()
	{
		// Arrange
		var outputTypes = new List<OutputType>
		{
			_converter.Convert(typeof(ReportType))
		};

		var apiClient = new ApiClient("TestClient", "TestController", "test", null);
		apiClient.AddEndpoint(new ApiClientEndpoint("getLatestId", "latest/{year}", EndpointMethod.GET, null, typeof(Guid), false,
				[new EndpointParameter("year", typeof(int), null, false, false, true, false, false, false, false, false),
				 new EndpointParameter("reportType", typeof(ReportType), null, false, false, false, true, false, false, false, false)], null));

		// Act
		var result = Sut.Write(apiClient, outputTypes, _converter, true, _templateFn, Casing.Pascal);

		// Assert
		var file = File.ReadAllText(result).Trim();
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("export class TestClient {")
			.And.Contain("public async getLatestId(year: number, reportType: ReportType, cancellationToken: AbortSignal = null): Promise<string> {")
			.And.Contain("const url = new URL(`test/latest/${year}`, window.location.origin);")
			.And.NotContain("if (!!reportType)")
			.And.Contain("url.searchParams.append('reportType', reportType.toString());");
	}

	[Fact]
	public void Unpacks_Complex_Object_To_Query()
	{
		// Arrange
		var outputTypes = new List<OutputType>
		{
			_converter.Convert(typeof(PaginatedRequest))
		};

		var apiClient = new ApiClient("TestClient", "TestController", "test", null);
		apiClient.AddEndpoint(new ApiClientEndpoint("getLatestId", "latest", EndpointMethod.GET, null, typeof(Guid), false, [new EndpointParameter("request", typeof(PaginatedRequest), null, false, false, false, true, false, false, false, false)], null));

		// Act
		var result = Sut.Write(apiClient, outputTypes, _converter, true, _templateFn, Casing.Pascal);

		// Assert
		var file = File.ReadAllText(result).Trim();
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("import { PaginatedRequest }")
			.And.Contain("export class TestClient {")
			.And.Contain("public async getLatestId(request: PaginatedRequest, cancellationToken: AbortSignal = null): Promise<string> {")
			.And.Contain("const url = new URL('test/latest', window.location.origin);")
			.And.NotContain("url.searchParams.append('request'")
			.And.Contain("url.searchParams.append('year', request.year.toString());")
			.And.Contain("url.searchParams.append('page', request.page.toString());")
			.And.Contain("url.searchParams.append('pageSize', request.pageSize.toString());");
	}

	[Fact]
	public void Handles_Optional_Route_Parameter()
	{
		// Arrange
		var apiClient = new ApiClient("TestClient", "TestController", "test", null);
		apiClient.AddEndpoint(new ApiClientEndpoint("getLatestId", "latest/{year?}", EndpointMethod.GET, null, typeof(Guid), false, [new EndpointParameter("year", typeof(int), null, false, false, true, false, false, false, false, true)], null));

		// Act
		var result = Sut.Write(apiClient, [], _converter, true, _templateFn, Casing.Pascal);

		// Assert
		var file = File.ReadAllText(result).Trim();
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("export class TestClient {")
			.And.Contain("public async getLatestId(year: number | undefined, cancellationToken: AbortSignal = null): Promise<string> {")
			.And.Contain("const url = new URL('test/latest/{year?}', window.location.origin);")
			.And.Contain("if (year != undefined)")
			.And.Contain("url.pathname = url.pathname.replace('{year?}', year.toString());")
			.And.Contain("else")
			.And.Contain("url.pathname = url.pathname.replace('/{year?}', '');")
			.And.NotContain("url.searchParams.append(");
	}

	[Theory]
	[InlineData(Casing.Camel)]
	[InlineData(Casing.Pascal)]
	[InlineData(Casing.Kebab)]
	[InlineData(Casing.Snake)]
	public void Changes_File_Name_According_To_Casing(Casing casing)
	{
		// Arrange
		var apiClient = new ApiClient("TestClient", "TestController", "test", null);
		apiClient.AddEndpoint(new ApiClientEndpoint("getLatestId", "latest", EndpointMethod.GET, typeof(Guid), typeof(Guid), false, [], null));

		// Act
		var result = Sut.Write(apiClient, [], _converter, false, _templateFn, casing);

		// Assert
		switch (casing)
		{
			case Casing.Camel:
				result.Should().EndWith("testClient.ts");
				break;
			case Casing.Pascal:
				result.Should().EndWith("TestClient.ts");
				break;
			case Casing.Kebab:
				result.Should().EndWith("test-client.ts");
				break;
			case Casing.Snake:
				result.Should().EndWith("test_client.ts");
				break;
		}
	}

	[Fact]
	public void Throws_Exception_When_Casing_Not_Supported()
	{
		// Arrange
		var apiClient = new ApiClient("TestClient", "TestController", "test", null);
		apiClient.AddEndpoint(new ApiClientEndpoint("getLatestId", "latest", EndpointMethod.GET, typeof(Guid), typeof(Guid), false, [], null));

		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => Sut.Write(apiClient, [], _converter, false, _templateFn, (Casing)4));
	}

	[Fact]
	public void Avoids_Duplicate_Imports_With_Same_ReturnType_And_Parameter()
	{
		// Arrange
		var outputTypes = new List<OutputType>
		{
			_converter.Convert(typeof(PersonDto))
		};

		var apiClient = new ApiClient("TestClient", "TestController", "test", null);
		apiClient.AddEndpoint(new ApiClientEndpoint("update", "", EndpointMethod.PATCH, typeof(PersonDto), typeof(PersonDto), false, [
			new EndpointParameter("request", typeof(PersonDto), null, false, true, false, false, false, false, false, false)
		], null));

		// Act
		var result = Sut.Write(apiClient, outputTypes, _converter, true, _templateFn, Casing.Pascal);

		// Assert
		var file = File.ReadAllText(result).Trim();
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { PersonDto, PersonDtoSchema } from '~/type-contractor/tests/type-script/person-dto';")
			.And.NotContain("import { PersonDto } from '~/type-contractor/tests/type-script/person-dto';")
			.And.Contain("export class TestClient {")
			.And.Contain("public async update(request: PersonDto, cancellationToken: AbortSignal = null): Promise<PersonDto> {")
			.And.Contain("const url = new URL('test', window.location.origin);")
			.And.NotContain("url.searchParams.append(")
			.And.Contain("return await response.parseJson<PersonDto>(PersonDtoSchema);");
	}

	private class PaginatedRequest
	{
		public int Year { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
	}

	private enum ReportType
	{
		Summary = 0,
		Detailed = 1,
	}

	private record PersonDto(Guid Id, string FirstName, string LastName);

	private MetadataLoadContext BuildMetadataLoadContext()
	{
		// Get the array of runtime assemblies.
		var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");

		// Create the list of assembly paths consisting of runtime assemblies and the inspected assemblies.
		var paths = runtimeAssemblies.Concat(_configuration.Assemblies.Values);

		var resolver = new PathAssemblyResolver(paths);

		return new MetadataLoadContext(resolver);
	}

	public void Dispose()
	{
		if (_outputDirectory.Exists)
			_outputDirectory.Delete(true);
	}
}
