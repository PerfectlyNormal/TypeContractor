using TypeContractor.Output;

namespace TypeContractor.Templates;
public record RouteParameterTemplateDto(string Name, bool IsOptional)
{
	internal static RouteParameterTemplateDto Build(EndpointParameter x)
		=> new(x.Name, x.IsOptional);
}
