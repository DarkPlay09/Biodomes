namespace BioDomes.Web.Middlewares;

public class ReverseProxyLinksMiddleware: IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Headers.TryGetValue("X-Forwarded-Prefix", out var pathBase))
        {
            context.Request.PathBase = pathBase[^1];
            if (context.Request.Path.StartsWithSegments(context.Request.PathBase, out var path))
            {
                context.Request.Path = path;
            }
        }
        return Task.Run(() => next(context));
    }
}