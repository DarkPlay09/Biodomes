namespace BioDomes.Web.Middlewares;

public static class ReverseProxyLinksMiddlewareExtension
{
    public static IApplicationBuilder UseReverseProxyLinks(this IApplicationBuilder app) 
        => app.UseMiddleware<ReverseProxyLinksMiddleware>();
}