using DynamicApi.Configurations;
using DynamicApi.Services.Listener;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DynamicApi.EntityFramework;

public class EntityHandler {

    private readonly AsyncServiceScope _scope;
    private readonly ChangeTracker _changeTracker;
    private readonly DynamicContext _context;

    public EntityHandler(AsyncServiceScope scope, ChangeTracker changeTracker, DynamicContext context) {
        _scope = scope;
        _changeTracker = changeTracker;
        _context = context;
    }

    public async Task<List<Func<Task>>> OnSaving() {
        var entityEntries = GetEntityEntries();
        var functions = GetEntityFunctions(entityEntries);
        return await ExecuteEntityFunctions(functions);
    }

    private IEnumerable<EntityEntry> GetEntityEntries() {
        return _changeTracker.Entries().AsParallel()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();
    }

    private List<Func<Task<Func<Task>>>> GetEntityFunctions(IEnumerable<EntityEntry> entityEntries) {
        return entityEntries.SelectMany(GetEntityServices).ToList();
    }

    private IEnumerable<Func<Task<Func<Task>>>> GetEntityServices(EntityEntry entityEntry) {
        var entity = entityEntry.Entity;
        var type = Unproxy(entity.GetType());
        var state = entityEntry.State;
        var serviceInfos = Configuration.Listeners.Where(x =>
            x.ListenerInfo.ModelType.IsAssignableTo(type) || x.ListenerInfo.ModelType.IsAssignableFrom(type)).ToList();

        foreach (var info in serviceInfos) {
            if(_scope.ServiceProvider.GetService(info.ServiceType) is not IListenerService service) {
                throw new Exception($"Service {info.ServiceType} is not a IListenerService");
            }

            yield return async () => await service!.Handle(entity, info.ListenerInfo.Configuration, state, _context);
        }
    }

    private async Task<List<Func<Task>>> ExecuteEntityFunctions(List<Func<Task<Func<Task>>>> functions) {
        var onSaved = new List<Func<Task>>();

        foreach (var function in functions) {
            var savedHandle = await function();

            if(savedHandle != null) {
                onSaved.Add(savedHandle);
            }
        }

        return onSaved;
    }

    public static Type Unproxy(Type type) {
        return type.Namespace == "Castle.Proxies" ? type.BaseType : type;
    }

}