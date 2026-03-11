using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using BioDomes.Domains;
using BioDomes.Domains.Extensions;

namespace BioDomes.Web.Routing;

public class SpeciesSlugConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey,
        RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (routeDirection == RouteDirection.UrlGeneration)
            return true;

        if (!values.TryGetValue(routeKey, out var raw) || raw is null) return false;

        var s = raw.ToString();
        if (string.IsNullOrWhiteSpace(s)) return false;
        
        return s == s.ToKebabCase();
    }
}