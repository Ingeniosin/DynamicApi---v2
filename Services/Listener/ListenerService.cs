using DynamicApi.EntityFramework;
using DynamicApi.Services.Handlers;
using Microsoft.EntityFrameworkCore;

namespace DynamicApi.Services.Listener;

public abstract class ListenerService<T, TDbContext> : IListenerService
    where T : class where TDbContext : DynamicContext {

    #region methods

    public virtual Task OnCreating(T entity, TDbContext context) {
        throw new NotImplementedException();
    }

    public virtual Task OnCreated(T entity, TDbContext context) {
        throw new NotImplementedException();
    }

    public virtual Task OnUpdating(T entity, T oldModel, Func<string, bool> isChanged, TDbContext context) {
        throw new NotImplementedException();
    }

    public virtual Task OnUpdated(T entity, T oldModel, Func<string, bool> isChanged, TDbContext context) {
        throw new NotImplementedException();
    }

    public virtual Task OnDeleting(T entity, TDbContext context) {
        throw new NotImplementedException();
    }

    public virtual Task OnDeleted(T entity, TDbContext context) {
        throw new NotImplementedException();
    }

    public virtual Task Normalize(T entity, TDbContext context) {
        throw new NotImplementedException();
    }

    #endregion


    public async Task<Func<Task>> Handle(object model, ListenerConfiguration configuration, EntityState state,
        DynamicContext context) {
        var typpedModel = (T)model;
        var dynamicDbContext = (TDbContext)context;

        if(configuration.Normalize) {
            await Normalize(typpedModel, dynamicDbContext);
        }

        var handler =
            HandlerFactory<T, TDbContext>.CreateHandler(state, configuration, typpedModel, dynamicDbContext, this);
        return await handler.Handle();
    }

}