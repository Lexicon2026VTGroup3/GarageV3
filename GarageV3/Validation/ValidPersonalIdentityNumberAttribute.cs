using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GarageV3.Validation;

public class ValidPersonalIdentityNumberAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string pin || string.IsNullOrWhiteSpace(pin))
        {
            return new ValidationResult("Personal identity number is required.");
        }

        string cleaned = pin.Replace("-", "").Replace("+", "").Trim();

        if (!Regex.IsMatch(cleaned, @"^\d{12}$"))
        {
            return new ValidationResult("Personal identity number must follow the format YYYYMMDD-XXXX or YYYYMMDDXXXX.");
        }

        string datePart = cleaned[..8];
        if (!DateTime.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            return new ValidationResult("Personal identity number must contain a valid date.");
        }

        return ValidationResult.Success;
    }
}
