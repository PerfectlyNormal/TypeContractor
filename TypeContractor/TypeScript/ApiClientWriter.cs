using HandlebarsDotNet;
using System.Text;
using System.Text.RegularExpressions;
using TypeContractor.Helpers;
using TypeContractor.Logger;
using TypeContractor.Output;
using TypeContractor.Templates;

namespace TypeContractor.TypeScript;

public partial class ApiClientWriter(string outputPath, string? relativeRoot)
{
	private static readonly Encoding _utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
	private static readonly Dictionary<EndpointMethod, string> _httpMethods = new()
	{
		{ EndpointMethod.GET, "get" },
		{ EndpointMethod.POST, "post" },
		{ EndpointMethod.PUT, "put" },
		{ EndpointMethod.PATCH, "patch" },
		{ EndpointMethod.DELETE, "delete" },
	};

	[GeneratedRegex(@"([^\$])\{([A-Za-z0-9]+)\}")]
	private static partial Regex RouteParameterRegex();

	public string Write(ApiClient apiClient, IEnumerable<OutputType> allTypes, TypeScriptConverter converter, bool buildZodSchema, HandlebarsTemplate<object, ApiClientTemplateDto> template)
	{
		var _builder = new StringBuilder();
		ArgumentNullException.ThrowIfNull(apiClient);

		Log.Instance.LogDebug($"Processing controller {apiClient.Name}");

		var directory = Path.Combine(outputPath, "clients");
		var filePath = Path.Combine(directory, $"{apiClient.Name}.ts");

		// Handle endpoints
		var endpoints = new List<EndpointTemplateDto>(apiClient.Endpoints.Count());
		foreach (var endpoint in apiClient.Endpoints)
		{
			Log.Instance.LogDebug($"  Processing endpoint {endpoint.Name}");
			var url = !string.IsNullOrWhiteSpace(apiClient.Prefix) && !endpoint.Route.StartsWith('/') && !endpoint.Route.StartsWith("~/", StringComparison.Ordinal)
				? $"{apiClient.Prefix}/{endpoint.Route}"
				: endpoint.Route.Replace("~/", string.Empty);
			if (!_httpMethods.TryGetValue(endpoint.Method, out var method))
				throw new NotImplementedException($"No mapping exists for {endpoint.Method}");

			var parameters = endpoint.Parameters.Select(x => MapParameter(x, converter)).ToList();
			var parameterMap = parameters.Select(x => $"{x.ParameterName}{((x.Type?.IsNullable ?? false) && !x.IsOptional ? "?" : "")}: {x.Type?.FullTypeName ?? "any"}{(x.IsOptional ? " | undefined" : "")}").ToList();
			var returnType = (endpoint.ReturnType is null
					? null
					: converter.GetDestinationType(endpoint.ReturnType, endpoint.ReturnType.CustomAttributes, false, TypeChecks.IsNullable(endpoint.ReturnType))?.FullTypeName) ?? "Response";

			var routeParams = endpoint.Parameters
				.Where(x => x.FromRoute)
				.Select(RouteParameterTemplateDto.Build)
				.ToList();
			var dynamicUrl = routeParams.Any(x => !x.IsOptional);
			if (dynamicUrl)
			{
				foreach (var param in routeParams.Where(x => !x.IsOptional))
					url = url.Replace($"{{{param.Name}}}", $"${{{param.Name}}}");
			}

			if (url.EndsWith('/'))
				url = url[..^1];

			var regex = RouteParameterRegex();
			if (regex.IsMatch(url))
			{
				var matches = regex.Matches(url);
				var names = matches.Select(x => x.Groups[2].Value);
				Log.Instance.LogError($"URL for {apiClient.Name}.{endpoint.Name} contains unmatched route parameters: {string.Join(", ", names)}");
			}

			var queryParams = endpoint.Parameters.Where(x => x.FromQuery).ToList();
			var queryParamsDto = new List<QueryParameterTemplateDto>(queryParams.Count);
			foreach (var queryParam in queryParams)
			{
				var destinationType = converter.GetDestinationType(queryParam.ParameterType, queryParam.ParameterType.CustomAttributes, false, TypeChecks.IsNullable(queryParam.ParameterType));
				if (destinationType.IsBuiltin)
				{
					queryParamsDto.Add(new QueryParameterTemplateDto(queryParam.Name, destinationType.IsBuiltin, destinationType.IsNullable, destinationType.IsArray, queryParam.IsOptional, null));
				}
				else
				{
					var outputType = allTypes.FirstOrDefault(x => x.FullName == destinationType.FullName);
					if (outputType is null)
					{
						Log.Instance.LogWarning($"Unable to find {destinationType.FullName} in the converted types. Skipping!");
						continue;
					}

					foreach (var property in outputType.Properties ?? [])
					{
						queryParamsDto.Add(new QueryParameterTemplateDto(queryParam.Name, false, property.IsNullable, false, queryParam.IsOptional, property.DestinationName));
					}
				}
			}

			var requiresBody = endpoint.Method is EndpointMethod.POST or EndpointMethod.PUT or EndpointMethod.PATCH;
			var body = endpoint.Parameters.FirstOrDefault(p => p.FromBody || (!p.FromRoute && !p.FromQuery));

			var returnUnparsedResponse = endpoint.UnwrappedReturnType is null && endpoint.ReturnType is null;
			var targetType = buildZodSchema && endpoint.ReturnType is not null
				? converter.GetDestinationType(endpoint.ReturnType, endpoint.ReturnType.CustomAttributes, false, TypeChecks.IsNullable(endpoint.ReturnType))
				: null;

			endpoints.Add(new EndpointTemplateDto(
				endpoint.Name,
				endpoint.Obsolete is not null,
				endpoint.Obsolete?.Reason ?? "",
				method,
				returnType,
				endpoint.UnwrappedReturnType?.Name,
				endpoint.EnumerableReturnType,
				targetType?.TypeName,
				targetType?.IsArray,
				url,
				dynamicUrl,
				routeParams,
				parameterMap,
				queryParamsDto,
				requiresBody,
				body?.Name,
				returnUnparsedResponse
				));
		}

		var hasJsonParameter = apiClient.Endpoints.Any(x => x.Parameters.Any(p => p.FromBody || (!p.FromRoute && !p.FromQuery)));
		var data = new ApiClientTemplateDto(
			apiClient.Name,
			hasJsonParameter,
			BuildImports(apiClient.Endpoints, allTypes, converter, buildZodSchema),
			apiClient.Obsolete is not null,
			apiClient.Obsolete?.Reason ?? "",
			endpoints,
			buildZodSchema
			);

		var result = template(data);

		// Create directory if needed
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);

		// Write file
		File.WriteAllText(filePath, result.Trim() + Environment.NewLine, _utf8WithoutBom);

		// Return the path we wrote to
		return filePath;
	}

	private static (string ParameterName, DestinationType? Type, bool IsOptional) MapParameter(EndpointParameter parameter, TypeScriptConverter converter)
	{
		Log.Instance.LogDebug($"Mapping parameter {parameter.Name} ({parameter.ParameterType.Name})");

		var targetType = converter.GetDestinationType(parameter.ParameterType, parameter.ParameterType.CustomAttributes, false, TypeChecks.IsNullable(parameter.ParameterType));
		return (parameter.Name, targetType, parameter.IsOptional);
	}

	private List<string> BuildImports(IEnumerable<ApiClientEndpoint> endpoints, IEnumerable<OutputType> allTypes, TypeScriptConverter converter, bool buildZodSchema)
	{
		var imports = new List<string>();
		var needZodLibrary = false;

		foreach (var endpoint in endpoints)
		{
			var returnType = endpoint.ReturnType is null
				? null
				: converter.GetDestinationType(endpoint.ReturnType, endpoint.ReturnType.CustomAttributes, false, TypeChecks.IsNullable(endpoint.ReturnType));

			if (returnType is not null && returnType.IsBuiltin)
			{
				needZodLibrary = true;
			}
			else if (returnType is not null && !returnType.IsBuiltin)
			{
				var importTypes = new List<string> { returnType.ImportType };
				if (buildZodSchema)
				{
					var zodImport = ZodSchemaWriter.BuildImport(returnType);

					if (!string.IsNullOrWhiteSpace(zodImport))
						importTypes.Add(zodImport);
				}

				var outputType = allTypes.First(x => x.FullName == (returnType.InnerType?.FullName ?? returnType.FullName));
				var importPath = $"{relativeRoot}/{outputType.ContractedType.Folder.Path.Replace('\\', '/')}/{outputType.Name}";
				var returnImport = $"import {{ {string.Join(", ", importTypes)} }} from '{importPath}';";
				imports.Add(returnImport);
			}

			var parameters = endpoint.Parameters.Select(x => MapParameter(x, converter)).ToList();
			foreach (var parameter in parameters)
			{
				if (parameter.Type?.IsBuiltin ?? true)
					continue;

				var outputType = allTypes.First(x => x.FullName == (parameter.Type.InnerType?.FullName ?? parameter.Type.FullName));
				var importPath = $"{relativeRoot}/{outputType.ContractedType.Folder.Path.Replace('\\', '/')}/{outputType.Name}";
				var parameterImport = $"import {{ {parameter.Type.ImportType} }} from '{importPath}';";
				imports.Add(parameterImport);
			}
		}

		if (buildZodSchema && needZodLibrary)
			imports.Insert(0, ZodSchemaWriter.LibraryImport);

		return imports.Distinct().ToList();
	}
}
