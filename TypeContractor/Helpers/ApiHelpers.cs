using System.Reflection;
using System.Text.RegularExpressions;
using TypeContractor.Annotations;
using TypeContractor.Logger;
using TypeContractor.Output;
using TypeContractor.TypeScript;
using static TypeContractor.Helpers.TypeChecks;

namespace TypeContractor.Helpers;

public static partial class ApiHelpers
{
	private static readonly Regex _routeParameterRegex = RouteParameterRegexImpl();

	[GeneratedRegex("{([A-Za-z]+)(:[[A-Za-z]+)?}")]
	private static partial Regex RouteParameterRegexImpl();

	public static ApiClient? BuildApiClient(Type controller, List<MethodInfo> endpoints)
	{
		var ignoreAttribute = controller.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == typeof(TypeContractorIgnoreAttribute).FullName);
		if (ignoreAttribute is not null)
		{
			Log.Instance.LogDebug($"Controller {controller.Name} marked with Ignore. Skipping.");
			return null;
		}

		// Find route prefix, if any
		var prefixAttribute = controller.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.RouteAttribute");
		var prefix = prefixAttribute?.ConstructorArguments.First().Value as string;

		// Handle obsolete
		var obsoleteAttribute = controller.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.ObsoleteAttribute");
		var obsoleteInfo = obsoleteAttribute is not null ? new ObsoleteInfo(obsoleteAttribute.ConstructorArguments.FirstOrDefault().Value as string) : null;

		// Find name of the client
		var clientAttribute = controller.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == typeof(TypeContractorNameAttribute).FullName);
		var clientName = clientAttribute?.ConstructorArguments.FirstOrDefault().Value as string
					  ?? controller.Name.Replace("Controller", "Client");

		var client = new ApiClient(clientName, controller.FullName!, prefix, obsoleteInfo);

		foreach (var endpoint in endpoints)
		{
			foreach (var apiEndpoint in BuildApiEndpoint(endpoint))
				client.AddEndpoint(apiEndpoint);
		}

		return client;
	}

	internal static List<ApiClientEndpoint> BuildApiEndpoint(MethodInfo endpoint)
	{
		// Find HTTP method
		var httpAttributes = endpoint
			.CustomAttributes
			.Where(IsHttpAttribute);

		var endpoints = new List<ApiClientEndpoint>(httpAttributes.Count());
		foreach (var method in httpAttributes)
		{
			var (route, httpMethod) = DetermineRoute(endpoint, method);

			// Handle obsolete
			var endpointObsoleteAttribute = endpoint.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.ObsoleteAttribute");

			// Handle return type and input parameters
			var returnType = UnwrappedReturnType(endpoint);
			var parameters = endpoint.GetParameters()
				.Select(p => new EndpointParameter(
					p.Name!,
					p.ParameterType,
					UnwrappedResult(p.ParameterType),
					ImplementsIEnumerable(p.ParameterType),
					FromBody: p.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.FromBodyAttribute"),
					FromRoute: ParameterIsFromRoute(p, route),
					FromQuery: ParameterIsFromQuery(p, httpMethod, route),
					FromHeader: p.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.FromHeaderAttribute"),
					FromServices: p.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.FromServicesAttribute"),
					FromForm: p.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.FromFormAttribute"),
					IsOptional: ParameterIsOptional(p, route)
				))
				.Where(x => !x.FromHeader && !x.FromServices && !x.FromForm)
				.Where(x => x.ParameterType.FullName != "System.Threading.CancellationToken")
				.ToList();


			var nameAttribute = endpoint.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == typeof(TypeContractorNameAttribute).FullName);
			var endpointName = nameAttribute?.ConstructorArguments.FirstOrDefault().Value as string
							?? endpoint.Name.ToTypeScriptName();
			Log.Instance.LogDebug($"Found endpoint {endpoint.Name}->{endpointName} returning {returnType?.Name ?? "HTTP"} with {parameters.Count} parameters");
			var apiEndpoint = new ApiClientEndpoint(endpointName,
													route,
													httpMethod,
													FullyUnwrappedReturnType(endpoint),
													returnType,
													returnType is not null && ImplementsIEnumerable(returnType),
													parameters,
													endpointObsoleteAttribute is not null ? new ObsoleteInfo(endpointObsoleteAttribute.ConstructorArguments.FirstOrDefault().Value as string) : null);
			endpoints.Add(apiEndpoint);
		}

		return endpoints;
	}

	private static (string Route, EndpointMethod HttpMethod) DetermineRoute(MethodInfo endpoint, CustomAttributeData method)
	{
		// Find route (HttpX or Route)
		var routeAttribute = endpoint.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.RouteAttribute");
		var route = routeAttribute?.ConstructorArguments.First().Value as string;
		var methodRoute = method.ConstructorArguments.Count == 1 ? method.ConstructorArguments.First().Value as string : null;
		var finalRoute = methodRoute ?? route ?? "";
		var matches = _routeParameterRegex.Matches(finalRoute);
		foreach (Match match in matches)
		{
			if (!match.Success) continue;
			if (match.Groups.Count < 3) continue;
			finalRoute = finalRoute.Replace(match.Value, $"{{{match.Groups[1].Value}}}");
		}

		var httpMethod = method.AttributeType.Name switch
		{
			"HttpGetAttribute" => EndpointMethod.GET,
			"HttpPostAttribute" => EndpointMethod.POST,
			"HttpPutAttribute" => EndpointMethod.PUT,
			"HttpPatchAttribute" => EndpointMethod.PATCH,
			"HttpDeleteAttribute" => EndpointMethod.DELETE,
			"HttpOptionsAttribute" => EndpointMethod.OPTIONS,
			"HttpHeadAttribute" => EndpointMethod.HEAD,
			_ => EndpointMethod.Invalid,
		};

		return (finalRoute, httpMethod);
	}

	private static bool ParameterIsFromRoute(ParameterInfo parameterInfo, string finalRoute)
	{
		if (parameterInfo.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.FromRouteAttribute"))
			return true;

		return finalRoute.Contains($"{{{parameterInfo.Name}}}") || finalRoute.Contains($"{{{parameterInfo.Name}?}}");
	}

	private static bool ParameterIsOptional(ParameterInfo parameterInfo, string finalRoute)
	{
		if (!ParameterIsFromRoute(parameterInfo, finalRoute))
			return false;

		return finalRoute.Contains($"{{{parameterInfo.Name}?}}");
	}

	private static bool ParameterIsFromQuery(ParameterInfo parameterInfo, EndpointMethod httpMethod, string finalRoute)
	{
		if (parameterInfo.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.FromQueryAttribute"))
			return true;

		var noBody = httpMethod is EndpointMethod.GET or EndpointMethod.DELETE;
		var name = parameterInfo.Name!;
		var hasRouteAttribute = parameterInfo.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.FromRouteAttribute");
		var parameterInRoute = finalRoute.Contains($"{{{parameterInfo.Name}}}");

		if (hasRouteAttribute || parameterInRoute)
			return false;

		return noBody;
	}
}
