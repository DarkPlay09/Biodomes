using BioDomes.Domains;
using BioDomes.Domains.Extensions;
using Microsoft.AspNetCore.Routing;

namespace BioDomes.Web.Transformers;

public class SlugTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        return value?.ToString()?.ToKebabCase();
    }
}