namespace TypeContractor.Templates;

public record ApiClientTemplateDto(
	string Name,
	bool HasJsonParameter,
	IEnumerable<string> Imports,
	bool IsObsolete,
	string ObsoleteReason,
	IEnumerable<EndpointTemplateDto> Endpoints,
	bool BuildZodSchema,
	string TypeContractorVersion);
