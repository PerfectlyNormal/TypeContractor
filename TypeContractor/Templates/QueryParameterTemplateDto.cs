namespace TypeContractor.Templates;

public record QueryParameterTemplateDto(
	string Name,
	bool IsBuiltin,
	bool IsNullable,
	bool IsArray,
	bool IsOptional,
	string? DestinationName);
