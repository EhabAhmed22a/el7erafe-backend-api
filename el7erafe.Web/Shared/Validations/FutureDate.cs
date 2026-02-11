
using System.ComponentModel.DataAnnotations;

namespace Shared.Validations
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateOnly date)
            {
                if (date < DateOnly.FromDateTime(DateTime.Today))
                {
                    return new ValidationResult(ErrorMessage ?? "التاريخ يجب أن يكون اليوم أو في المستقبل");
                }
            }
            return ValidationResult.Success;
        }
    }
}
