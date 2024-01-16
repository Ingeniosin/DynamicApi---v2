using DynamicApi.DevExpress;

namespace DynamicApi.Routes.Types;

public class ViewRoute : Route {

    public Type Type { get; }

    public ViewRoute(string name, Type type) : base("view/" + name) {
        Type = type;
    }

    public override void Load(WebApplication application, ILogger logger) {
        application.MapGet(Name, async (DataSourceLoadOptions loadOptions, HttpContext context) => {
            using var scope = application.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService(Type) as ViewRouteImpl;

            if(service == null) {
                throw new Exception("Could not find service");
            }

            return await service.Get(loadOptions, context);
        });
        logger.LogInformation($"Loaded {Name}");
    }

}