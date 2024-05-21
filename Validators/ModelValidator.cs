using System.ComponentModel.DataAnnotations;
using DynamicApi.EntityFramework;
using DynamicApi.Serializers;

namespace DynamicApi.Validators;

public class ModelValidator {

    public static void TryValidateModel(object model, out bool isValid, out IResult result) {
        isValid = true;
        result = null;

        var validator = new DataAnnotationsValidator.DataAnnotationsValidator();
        var validationResults = new List<ValidationResult>();
        

        validator.TryValidateObject(model, validationResults); // TODO: cambiar por recursivo pero arreglar bug circular
        var type = EntityHandler.Unproxy(model.GetType());
        var results = validationResults.Where(validationResult => {
                var first = type.Name + "." + validationResult.MemberNames.First();
                var memberNames = first.Split(".");
                var lastMember = memberNames.Last();
                var hasVirtual = lastMember.EndsWith("Id");
                if(!hasVirtual) {
                    return true;
                }
                var virtualMember = lastMember.Replace("Id", "");
                return !memberNames.Contains(virtualMember);
            })
            .ToList();

        if(results.Count == 0) {
            return;
        }

        isValid = false;
        result = Serializer.Serialize(new {
            error = "An error occurred while validating the model.",
            details = results.Select(x => new {
                x.ErrorMessage,
                x.MemberNames
            })
        }, SerializeType.ERROR);
    }

}