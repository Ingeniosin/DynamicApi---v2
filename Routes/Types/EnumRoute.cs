using DevExtreme.AspNet.Data;
using DynamicApi.DevExpress;
using DynamicApi.Serializers;

namespace DynamicApi.Routes.Types;

public class EnumRoute<T> : Route where T : struct, Enum {

    private readonly IEnumerable<EnumItem> objects;

    public EnumRoute(string name) : base("types/" + name) {
        var enumerable = Enum.GetValues<T>();
        objects = enumerable.Select(x => new EnumItem {
            Id = Convert.ToInt32(x),
            Name = x.ToString()
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

    }

}