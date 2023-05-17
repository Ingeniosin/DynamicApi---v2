using System.ComponentModel.DataAnnotations;
using DynamicApi.Serializers;

namespace DynamicApi.Validators; 

public class ModelValidator {

    public static void TryValidateModel(object model, out bool isValid, out IResult result) {
        isValid = true;
        result = null;
        
        var validationContext = new ValidationContext(model);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(model, validationContext, validationResults, true);

        if(validationResults.Count == 0) {
            return;
        }
        
        var firstError = validationResults.FirstOrDefault();
        
        isValid = false;
        result = Serializer.Serialize(new {
            message = firstError?.ErrorMessage,
        }, SerializeType.ERROR);
    }
    

}