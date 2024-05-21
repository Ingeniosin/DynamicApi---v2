using System.Text;
using DynamicApi.Configurations;
using Newtonsoft.Json;

namespace DynamicApi.Serializers;

public class StandardSerializer : ISerializer {

    public IResult Serialize(object model) {
        return model != null
            ? Results.Text(JsonConvert.SerializeObject(model, Configuration.JsonConfigurations), "application/json", Encoding.UTF8)
            : Results.Ok();
    }

}