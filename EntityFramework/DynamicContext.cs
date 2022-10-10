using DynamicApi.Configurations;
using DynamicApi.Services.Listener;
using Microsoft.EntityFrameworkCore;

namespace DynamicApi.EntityFramework; 

public class DynamicContext : DbContext {

    public DynamicContext(DbContextOptions options) : base(options) {
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new()) {
        await using var scope = Configuration.ServiceProvider.CreateAsyncScope();
        var onSaving = await OnSaving(scope);
        var saveChangesAsync = await SaveChangesWithOutHandle();
        foreach (var func in onSaving) {
            await func();
        }
        return saveChangesAsync;
    }

    public Task<int> SaveChangesWithOutHandle() {
        return base.SaveChangesAsync(true, CancellationToken.None);
    }

    private async Task<List<Func<Task>>> OnSaving(IServiceScope scope) {
        var entries = ChangeTracker.Entries().AsParallel().Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).Select(entityEntry => {
            var entity = entityEntry.Entity;
            var type = Unproxy(entity.GetType());
            var state = entityEntry.State;
            var serviceInfos = Configuration.Listeners.Where(x => x.ListenerInfo.ModelType.IsAssignableTo(type) || x.ListenerInfo.ModelType.IsAssignableFrom(type)).ToList();
            var functions = new List<Func<Task<Func<Task>>>>();
            foreach (var info in serviceInfos) {
                if(scope.ServiceProvider.GetService(info.ServiceType) is not IListenerService service)
                    throw new Exception($"Service {info.ServiceType} is not a IListenerService");
                functions.Add(async () => await service!.Handle(entity, info.ListenerInfo.Configuration, state, this));
            }
            return functions;
        }).Aggregate(new List<Func<Task<Func<Task>>>>(), (list, funcs) => {
            list.AddRange(funcs);
            return list;
        });
        var onSaved = new List<Func<Task>>();
        foreach (var function in entries) {
            var savedHandle = await function();
            if (savedHandle != null) {
                onSaved.Add(savedHandle);
            }
        }
        
        return onSaved;
    }
    
    private static Type Unproxy(Type type) {
        return type.Namespace == "Castle.Proxies" ? type.BaseType : type;
    }
    
}

public static class DynamicExtensions {
    public static IQueryable<object> Set (this DbContext _context, Type t) {
        return (IQueryable<object>)_context.GetType().GetMethod("Set").MakeGenericMethod(t).Invoke(_context, null);
    }
}