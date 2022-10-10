using DynamicApi.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace DynamicApi.Services.Listener; 

public abstract class ListenerService<T, TDbContext> : IListenerService where T : class where TDbContext : DynamicContext {
    public virtual async Task OnCreating(T entity, TDbContext context){}
    public virtual async Task  OnCreated(T entity, TDbContext context){}
    
    public virtual async Task  OnUpdating(T entity, Func<bool, Task<T>> getOldModel, TDbContext context){}
    public virtual async Task  OnUpdated(T entity, Func<bool,Task<T>> getOldModel, TDbContext context){}
    
    public virtual async Task  OnDeleting(T entity, TDbContext context){}
    public virtual async Task  OnDeleted(T entity, TDbContext context){}
    
    public async Task<Func<Task>> Handle(object model, ListenerConfiguration configuration, EntityState state, DynamicContext context) {
        var typpedModel = (T)model;
        var dynamicDbContext = (TDbContext)context;
        switch (state) {
            case EntityState.Added: {
                if(configuration.OnCreating) await OnCreating(typpedModel, dynamicDbContext);
                return configuration.OnCreated ? async () => await OnCreated(typpedModel, dynamicDbContext) : null;
            }
            case EntityState.Modified when !configuration.OnUpdating && !configuration.OnUpdated:
                return null;
            case EntityState.Modified: {
                #region updateLogic
                var detachCheck = () => {};
                async Task<T> GetOldModel(bool withRelations) {
                    var oldModel = context.Entry(model).OriginalValues.ToObject() as T;
                    oldModel!.GetType().GetProperty("Id")!.SetValue(oldModel, 0);
                    await context.AddAsync(oldModel);
                    var entry = context.Entry(oldModel);
                    detachCheck = () => entry.State = EntityState.Detached;
                    return oldModel;
                }
                #endregion
                if(configuration.OnUpdating) await OnUpdating(typpedModel, GetOldModel, dynamicDbContext);
                detachCheck();
                if(!configuration.OnUpdated) return null;
                var entry = context.Entry(typpedModel);
                return async () => {
                    entry.State = EntityState.Detached;
                    context.Update(typpedModel);
                    await OnUpdated(typpedModel, GetOldModel, dynamicDbContext);
                    detachCheck();
                    await context.SaveChangesWithOutHandle();
                };
            }
            case EntityState.Deleted: {
                if(configuration.OnDeleting) await OnDeleting(typpedModel, dynamicDbContext);
                return configuration.OnDeleted ? async () => await OnDeleted(typpedModel, dynamicDbContext) : null;
            }
            case EntityState.Detached:
            case EntityState.Unchanged:
            default:
                return null;
        }
    }
}