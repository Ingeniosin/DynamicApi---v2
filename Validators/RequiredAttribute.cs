using System.ComponentModel.DataAnnotations;

namespace DynamicApi.Validators;

public class RequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute {

    protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
        var validationResult = base.IsValid(value, validationContext);

        if(validationResult == ValidationResult.Success) {
            return validationResult;
        }

        var property = validationContext.ObjectType.GetProperty(validationContext.MemberName.Replace("Id", ""));
        var isValid = property?.GetValue(validationContext.ObjectInstance) != null;

        if(isValid) {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });
    }

}