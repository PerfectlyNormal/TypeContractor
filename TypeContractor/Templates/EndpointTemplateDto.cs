namespace TypeContractor.Templates;

public record EndpointTemplateDto(
    string Name,
    bool IsObsolete,
    string ObsoleteReason,
    string HttpMethod,
    string ReturnType,
    string? UnwrappedReturnType,
    bool EnumerableReturnType,
    string? MappedReturnType,
    bool? EnumerableMappedReturnType,
    string Url,
    bool DynamicUrl,
    IEnumerable<string> Parameters,
    IEnumerable<QueryParameterTemplateDto> QueryParameters,
    bool RequiresBody,
    string? BodyParameter,
    bool ReturnUnparsedResponse);
