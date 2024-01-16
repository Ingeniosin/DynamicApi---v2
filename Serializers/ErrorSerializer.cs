namespace DynamicApi.Serializers;

public class ErrorSerializer : ISerializer {

    public IResult Serialize(object model) {
        return Results.Json(model, contentType: "application/json", statusCode: 400);
    }

}