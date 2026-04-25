using System.Globalization;
using System.Text;

namespace BioDomes.Infrastructures.Services.Slug;

public class SlugService : ISlugService
{
    public string ToSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace("'", "")
            .Replace("’", "")
            .Replace(" ", "-");
    }
}
