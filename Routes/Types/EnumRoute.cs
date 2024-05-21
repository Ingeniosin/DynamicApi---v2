using DevExtreme.AspNet.Data;
using DynamicApi.Configurations;
using DynamicApi.DevExpress;
using DynamicApi.Serializers;
using Newtonsoft.Json;

namespace DynamicApi.Routes.Types;

public class EnumRoute<T, D> : Route where T : struct, Enum {

    private readonly IEnumerable<EnumItem> objects;

    public EnumRoute(string name, IReadOnlyDictionary<T, D> data = null) : base("types/" + name) {
        var enumerable = Enum.GetValues<T>();
        objects = enumerable.Select(x => new EnumItem {
            Id = Convert.ToInt32(x),
            Name = JsonConvert.SerializeObject(x, Configuration.JsonConfigurations).Trim('"'),
            Data = data != null && data.TryGetValue(x, out var value) ? value : null
        });
    }

    public IResult Get(DataSourceLoadOptions loadOptions) {
        return Serializer.Serialize(DataSourceLoader.Load(objects, loadOptions));
    }

    public override void Load(WebApplication application, ILogger logger) {
        application.MapGet(Name, Get);
        logger.LogInformation($"Loaded {Name}");
    }

    private class EnumItem {

        public int Id { get; set; }
        public string Name { get; set; }
        public object Data { get; set; }

    }

}