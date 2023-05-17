using DynamicApi.EntityFramework;
using DynamicApi.Services.Listener;
using Microsoft.EntityFrameworkCore;

namespace DynamicApi.Services.Handlers;

public class HandlerFactory<T, TDbContext> where TDbContext : DynamicContext where T : class {

    public static EntityStateHandler<T, TDbContext> CreateHandler(EntityState state, ListenerConfiguration configuration, T typedModel, TDbContext context, ListenerService<T, TDbContext> service) {
        return state switch {
            EntityState.Added => new CreationHandler<T, TDbContext>(configuration, typedModel, context, service),
            EntityState.Modified => new ModificationHandler<T, TDbContext>(configuration, typedModel, context, service),
            EntityState.Deleted => new DeletionHandler<T, TDbContext>(configuration, typedModel, context, service),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }

}