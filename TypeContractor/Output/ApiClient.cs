namespace TypeContractor.Output;

public record ApiClient(string Name, string TypeName, string? Prefix, ObsoleteInfo? Obsolete)
{
	private readonly List<ApiClientEndpoint> _endpoints = [];

	public void AddEndpoint(ApiClientEndpoint endpoint) => _endpoints.Add(endpoint);
	public IEnumerable<ApiClientEndpoint> Endpoints => _endpoints.AsReadOnly();
}

public record ApiClientEndpoint(string Name,
								string Route,
								EndpointMethod Method,
								Type? UnwrappedReturnType,
								Type? ReturnType,
								bool EnumerableReturnType,
								IEnumerable<EndpointParameter> Parameters,
								ObsoleteInfo? Obsolete)
{
}

public record EndpointParameter(string Name,
								Type ParameterType,
								Type? UnwrappedParameterType,
								bool IsEnumerable,
								bool FromBody,
								bool FromRoute,
								bool FromQuery,
								bool FromHeader,
								bool FromServices,
								bool FromForm);

public enum EndpointMethod
{
	Invalid = -1,
	GET,
	POST,
	PUT,
	PATCH,
	DELETE,
	HEAD,
	OPTIONS,
}
