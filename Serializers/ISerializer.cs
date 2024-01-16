namespace DynamicApi.Serializers;

public interface ISerializer {

    public IResult Serialize(object obj);

}