namespace DynamicApi.Serializers;

public class NoneSerializer : ISerializer {

    public IResult Serialize(object obj) {
        return Results.Ok();
    }

}