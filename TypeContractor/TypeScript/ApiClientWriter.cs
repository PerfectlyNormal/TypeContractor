using System.Reflection.Metadata;
using System.Text;
using TypeContractor.Helpers;
using TypeContractor.Logger;
using TypeContractor.Output;

namespace TypeContractor.TypeScript;

#pragma warning disable CA1305 // Specify IFormatProvider
public class ApiClientWriter(string outputPath, string? relativeRoot)
{
    private readonly StringBuilder _builder = new();
    private static readonly Dictionary<EndpointMethod, string> _httpMethods = new()
    {
        { EndpointMethod.GET, "get" },
        { EndpointMethod.POST, "post" },
        { EndpointMethod.PUT, "put" },
        { EndpointMethod.PATCH, "patch" },
        { EndpointMethod.DELETE, "delete" },
    };

    public string Write(ApiClient apiClient, IEnumerable<OutputType> allTypes, TypeScriptConverter converter, bool buildZodSchema)
    {
        ArgumentNullException.ThrowIfNull(apiClient);

        Logger.Log.Instance.LogDebug($"Processing controller {apiClient.Name}");
        _builder.Clear();

        var directory = Path.Combine(outputPath, "clients");
        var filePath = Path.Combine(directory, $"{apiClient.Name}.ts");

        // Write header
        WriteHeader(apiClient);
        WriteImports(apiClient, allTypes, converter, buildZodSchema);

        // Build class
        _builder.AppendLine("");
        _builder.AppendDeprecationComment(apiClient.Obsolete, 0);
        _builder.AppendLine("@autoinject()");
        _builder.AppendFormat("export class {0} {{\r\n", apiClient.Name);
        _builder.AppendLine("  constructor(private http: HttpClient) {}");

        // Handle endpoints
        foreach (var endpoint in apiClient.Endpoints)
        {
            Logger.Log.Instance.LogDebug($"  Processing endpoint {endpoint.Name}");
            var url = !string.IsNullOrWhiteSpace(apiClient.Prefix) && !endpoint.Route.StartsWith('/')
                ? $"{apiClient.Prefix}/{endpoint.Route}"
                : endpoint.Route;
            if (!_httpMethods.TryGetValue(endpoint.Method, out var method))
                throw new NotImplementedException($"No mapping exists for {endpoint.Method}");

            _builder.AppendLine("");
            _builder.AppendDeprecationComment(endpoint.Obsolete);

            var parameters = endpoint.Parameters.Select(x => MapParameter(x, converter)).ToList();
            var parameterMap = string.Join(", ", parameters.Select(x => $"{x.ParameterName}{(x.Type?.IsNullable ?? false ? "?" : "")}: {x.Type?.FullTypeName ?? "any"}").ToList());
            var returnType = endpoint.ReturnType is null
                ? null
                : converter.GetDestinationType(endpoint.ReturnType, endpoint.ReturnType.CustomAttributes, false, TypeChecks.IsNullable(endpoint.ReturnType));

            _builder.AppendFormat("  public async {0}({1}cancellationToken: AbortSignal = null): Promise<{2}> {{\r\n",
                                  endpoint.Name,
                                  parameterMap.Length > 0 ? $"{parameterMap}, " : "",
                                  returnType?.FullTypeName ?? "Response");

            var routeParams = endpoint.Parameters.Where(x => x.FromRoute).ToList();
            if (routeParams.Count == 0)
                _builder.AppendFormat("    const url = new URL('{0}', window.location.origin);\r\n", url);
            else
            {
                foreach (var param in routeParams)
                    url = url.Replace($"{{{param.Name}}}", $"${{{param.Name}}}");
                _builder.AppendFormat("    const url = new URL(`{0}`, window.location.origin);\r\n", url);
            }

            var queryParams = endpoint.Parameters.Where(x => x.FromQuery);
            foreach (var queryParam in queryParams)
            {
                var destinationType = converter.GetDestinationType(queryParam.ParameterType, queryParam.ParameterType.CustomAttributes, false, TypeChecks.IsNullable(queryParam.ParameterType));
                if (destinationType.IsBuiltin)
                {
                    if (destinationType is not null && destinationType.IsNullable)
                        _builder.AppendFormat("    if (!!{0})\r\n  ", queryParam.Name);
                    _builder.AppendFormat("    url.searchParams.append('{0}', {0}.toString());\r\n", queryParam.Name);
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
                        if (property.IsNullable)
                            _builder.AppendFormat("    if (!!{0})\r\n  ", property.DestinationName);
                        _builder.AppendFormat("    url.searchParams.append('{0}', {0}.toString());\r\n", property.DestinationName);
                    }
                }
            }

            var requiresBody = endpoint.Method == EndpointMethod.POST || endpoint.Method == EndpointMethod.PUT || endpoint.Method == EndpointMethod.PATCH;
            var body = endpoint.Parameters.FirstOrDefault(p => p.FromBody || (!p.FromRoute && !p.FromQuery));
            var bodyParameter = body is not null ? $"json({body.Name})" : "null";

            _builder.AppendFormat("    const response = await this.http.{0}(`${{url.pathname}}${{url.search}}`, {1}{{ signal: cancellationToken }});\r\n",
                                  method,
                                  requiresBody ? $"{bodyParameter}, " : "");

            if (endpoint.UnwrappedReturnType is null && endpoint.ReturnType is null)
                _builder.AppendLine("    return response;");
            else if (buildZodSchema)
            {
                if (endpoint.UnwrappedReturnType is not null)
                {
                    _builder.AppendFormat("    return await response.parseJson<{0}{1}>({0}Schema{2});\r\n",
                                          endpoint.UnwrappedReturnType.Name,
                                          endpoint.EnumerableReturnType ? "[]" : "",
                                          endpoint.EnumerableReturnType ? ".array()" : "");
                }
                else if (endpoint.ReturnType is not null)
                {
                    var targetType = converter.GetDestinationType(endpoint.ReturnType, endpoint.ReturnType.CustomAttributes, false, TypeChecks.IsNullable(endpoint.ReturnType));
                    if (targetType is null)
                        _builder.AppendLine("    return response;");
                    else
                    {
                        _builder.AppendFormat("    return await response.parseJson(z.{0}(){1});\r\n", targetType.TypeName, targetType.IsArray ? ".array()" : "");
                    }
                }
            }
            else
                _builder.AppendLine("    return await response.json();");

            _builder.AppendLine("  }");
        }

        _builder.AppendLine("}");

        // Create directory if needed
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        // Write file
        File.WriteAllText(filePath, _builder.ToString());

        // Return the path we wrote to
        return filePath;
    }

    private void WriteHeader(ApiClient apiClient)
    {
        var needJson = apiClient.Endpoints.Any(x => x.Parameters.Any(p => p.FromBody || (!p.FromRoute && !p.FromQuery)));
        _builder.AppendLine("import { autoinject } from 'aurelia-framework';");
        _builder.AppendFormat("import {{ HttpClient{0} }} from 'aurelia-fetch-client';\r\n", needJson ? ", json" : "");
    }

    private void WriteImports(ApiClient apiClient, IEnumerable<OutputType> allTypes, TypeScriptConverter converter, bool buildZodSchema)
    {
        var imports = BuildImports(apiClient.Endpoints, allTypes, converter, buildZodSchema);
        foreach (var import in imports)
            _builder.AppendLine(import);
    }

    private static (string ParameterName, DestinationType? Type) MapParameter(EndpointParameter parameter, TypeScriptConverter converter)
    {
        Logger.Log.Instance.LogDebug($"Mapping parameter {parameter.Name} ({parameter.ParameterType.Name})");

        var targetType = converter.GetDestinationType(parameter.ParameterType, parameter.ParameterType.CustomAttributes, false, TypeChecks.IsNullable(parameter.ParameterType));
        if (targetType is not null)
            return (parameter.Name, targetType);

        return (parameter.Name, null);
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
                needZodLibrary = true;
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
                var returnImport = $"import {{ {string.Join(", ", importTypes)} }} from \"{importPath}\";";
                imports.Add(returnImport);
            }

            var parameters = endpoint.Parameters.Select(x => MapParameter(x, converter)).ToList();
            foreach (var parameter in parameters)
            {
                if (parameter.Type?.IsBuiltin ?? true)
                    continue;

                var outputType = allTypes.First(x => x.FullName == (parameter.Type.InnerType?.FullName ?? parameter.Type.FullName));
                var importPath = $"{relativeRoot}/{outputType.ContractedType.Folder.Path.Replace('\\', '/')}/{outputType.Name}";
                var parameterImport = $"import {{ {parameter.Type.ImportType} }} from '{importPath}'";
                imports.Add(parameterImport);
            }
        }

        if (needZodLibrary)
            imports.Insert(0, ZodSchemaWriter.LibraryImport);

        return imports.Distinct().ToList();
    }
}
#pragma warning restore CA1305 // Specify IFormatProvider
