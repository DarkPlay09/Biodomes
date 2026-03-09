using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BioDomes.Domains;

public static class StringExtensions
{
    public static string ToKebabCase(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        // 1) trim + lower
        var s = input.Trim().ToLowerInvariant();

        // 2) enlever accents
        s = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in s)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }
        s = sb.ToString().Normalize(NormalizationForm.FormC);

        // 3) tout ce qui n'est pas lettre/chiffre -> "-"
        s = Regex.Replace(s, @"[^a-z0-9]+", "-");

        // 4) éviter les "--" et trim des "-"
        s = Regex.Replace(s, @"-+", "-").Trim('-');

        return s;
    }
}