namespace DynamicApi.Serializers;

public class CustomSerializer : ISerializer {

    public IResult Serialize(object obj) {
        return obj as IResult;
    }

}