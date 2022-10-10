using System.Text;
using Newtonsoft.Json;

namespace DynamicApi.Serializers;

public class StandardSerializer : ISerializer {
    public IResult Serialize(object model) {
        return model != null ? Results.Text(JsonConvert.SerializeObject(model), "application/json", Encoding.UTF8) : Results.Ok();
    }
}