#nullable enable
using DynamicApi.EntityFramework;
using DynamicApi.Services.Listener;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DynamicApi.Services.Handlers;

public class ModificationHandler<T, TDbContext> : EntityStateHandler<T, TDbContext> where TDbContext : DynamicContext where T : class {

    public ModificationHandler(ListenerConfiguration _configuration, T model, TDbContext context, ListenerService<T, TDbContext> service) : base(_configuration, model, context, service) {
    }

    public override async Task<Func<Task>?> Handle() {
        if(!_configuration.OnUpdating && !_configuration.OnUpdated) {
            return null;
        }

        var oldModel = getOldModel(out var entityEntry);

        var changedFields = GetChangedFields();
        bool IsChanged(string field) => changedFields.Contains(field.ToLower());

        if(_configuration.OnUpdating) {
            await _service.OnUpdating(_model, oldModel, IsChanged, _context);
        }

        DetachEntity(entityEntry);

        if(!_configuration.OnUpdated) {
            return null;
        }

        var entry = _context.Entry(_model);
        return async () => {
            DetachEntity(entry);
            _context.Update(_model);
            await _service.OnUpdated(_model, oldModel, IsChanged, _context);
            DetachEntity(entityEntry);
            await _context.SaveChangesWithOutHandle();
        };
    }

    private T getOldModel(out EntityEntry entityEntry) {
        var oldModel = _context.Entry(_model).OriginalValues.ToObject() as T;
        oldModel!.GetType().GetProperty("Id")!.SetValue(oldModel, 0);
        _context.Add(oldModel);
        entityEntry = _context.Entry(oldModel);
        return oldModel;
    }

    private List<string> GetChangedFields() {
        var modified = _context.Entry(_model).Properties
            .Where(x => x.IsModified && x.OriginalValue != x.CurrentValue)
            .Select(x => x.Metadata.Name.ToLower())
            .ToList();
        return modified;
    }

    private static void DetachEntity(EntityEntry? entry) {
        if(entry != null) {
            entry.State = EntityState.Detached;
        }
    }
}