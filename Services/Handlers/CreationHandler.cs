using DynamicApi.EntityFramework;
using DynamicApi.Services.Listener;

namespace DynamicApi.Services.Handlers;

public class CreationHandler<T, TDbContext> : EntityStateHandler<T, TDbContext>
    where TDbContext : DynamicContext where T : class {

    public CreationHandler(ListenerConfiguration configuration, T model, TDbContext context,
        ListenerService<T, TDbContext> service) : base(configuration, model, context, service) {
    }

    public override async Task<Func<Task>> Handle() {
        if(_configuration.OnCreating) {
            await _service.OnCreating(_model, _context);
        }

        if(_configuration.OnCreated) {
            return async () => await _service.OnCreated(_model, _context);
        }

        return null;
    }

}