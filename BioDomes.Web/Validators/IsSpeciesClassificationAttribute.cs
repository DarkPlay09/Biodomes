using System.ComponentModel.DataAnnotations;
using BioDomes.Domains;

namespace BioDomes.Web.Validators;

public class IsSpeciesClassificationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var s = value?.ToString();
        if (string.IsNullOrWhiteSpace(s))
            return ValidationResult.Success;

        var ok = Enum.TryParse<SpeciesClassification>(s, ignoreCase: true, out var parsed)
                 && Enum.IsDefined(typeof(SpeciesClassification), parsed);
        
        return ok
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage ?? "Classification invalide.");
    }
}