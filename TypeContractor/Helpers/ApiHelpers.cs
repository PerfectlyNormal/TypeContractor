using System.Net;
using System.Reflection;
using TypeContractor.Logger;
using TypeContractor.Output;
using TypeContractor.TypeScript;
using static TypeContractor.Helpers.TypeChecks;

namespace TypeContractor.Helpers;

public static class ApiHelpers
{
    public static ApiClient BuildApiClient(Type controller, List<MethodInfo> endpoints)
    {
        // Find route prefix, if any
        var prefixAttribute = controller.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.RouteAttribute");
        var prefix = prefixAttribute?.ConstructorArguments.First().Value as string;

        // Handle obsolete
        var obsoleteAttribute = controller.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.ObsoleteAttribute");
        var obsoleteInfo = obsoleteAttribute is not null ? new ObsoleteInfo(obsoleteAttribute.ConstructorArguments.FirstOrDefault().Value as string) : null;

        var clientName = controller.Name.Replace("Controller", "Client");
        var client = new ApiClient(clientName, controller.FullName!, prefix, obsoleteInfo);

        foreach (var endpoint in endpoints)
            foreach (var apiEndpoint in BuildApiEndpoint(endpoint))
                client.AddEndpoint(apiEndpoint);

        return client;
    }

    private static List<ApiClientEndpoint> BuildApiEndpoint(MethodInfo endpoint)
    {
        // Find HTTP method
        var httpAttributes = endpoint
            .CustomAttributes
            .Where(IsHttpAttribute);

        var endpoints = new List<ApiClientEndpoint>(httpAttributes.Count());
        foreach (var method in httpAttributes)
        {
            // Handle obsolete
            var endpointObsoleteAttribute = endpoint.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.ObsoleteAttribute");

            // Find route (HttpX or Route)
            var routeAttribute = endpoint.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.RouteAttribute");
            var route = routeAttribute?.ConstructorArguments.First().Value as string;
            var methodRoute = method.ConstructorArguments.Count == 1 ? method.ConstructorArguments.First().Value as string : null;

            // Handle return type and input parameters
            var returnType = UnwrappedReturnType(endpoint);
            var parameters = endpoint.GetParameters()
                .Select(p => new EndpointParameter(
                    p.Name!,
                    p.ParameterType,
                    UnwrappedResult(p.ParameterType),
                    ImplementsIEnumerable(p.ParameterType),
                    FromBody: p.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.FromBodyAttribute"),
                    FromRoute: p.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.FromRouteAttribute"),
                    FromQuery: p.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.FromQueryAttribute"),
                    FromHeader: p.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.FromHeaderAttribute"),
                    FromServices: p.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.FromServicesAttribute"),
                    FromForm: p.CustomAttributes.Any(x => x.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.FromFormAttribute")
                ))
                .Where(x => !x.FromHeader && !x.FromServices && !x.FromForm)
                .Where(x => x.ParameterType.FullName != "System.Threading.CancellationToken")
                .ToList();

            Log.Instance.LogDebug($"Found endpoint {endpoint.Name} returning {returnType?.Name ?? "HTTP"} with {parameters.Count} parameters");

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

            var endpointName = endpoint.Name.ToTypeScriptName();
            var apiEndpoint = new ApiClientEndpoint(endpointName,
                                                    methodRoute ?? route ?? "",
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
}