using DynamicApi.Configurations;
using DynamicApi.EntityFramework;
using DynamicApi.Serializers;
using DynamicApi.Services.Listener;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DynamicApi.Routes.Types;

public class ServiceRoutes<T, TDbContext> : StoredRoute<T, TDbContext>
    where TDbContext : DynamicContext where T : class {

    private ListenerConfiguration ListenerConfiguration;

    public async Task<IResult> Post(HttpContext context, TDbContext db) {
        var dbSet = DbSet(db);
        var model = dbSet.CreateProxy();
        var values = context.Request.Form["values"];
        JsonConvert.PopulateObject(values, model, Configuration.JsonBypassConfiguration);
        await dbSet.AddAsync(model);
        await db.SaveChangesAsync();
        return Serializer.Ok();
    }

    public ServiceRoutes(string name, Func<TDbContext, DbSet<T>> dbSet, ListenerConfiguration listenerConfiguration) :
        base(name, dbSet) {
        ListenerConfiguration = listenerConfiguration;
    }

    public override void Load(WebApplication application, ILogger logger) {
        var nonService = new NonServiceRoutes<T, TDbContext>(Name, DbSet);
        application.MapGet(Name, nonService.Get);
        application.MapPost(Name, nonService.Post);
        application.MapPut(Name, nonService.Put);
        application.MapDelete(Name, nonService.Delete);
        logger.LogInformation($"Loaded {Name}");

    }

}