using DynamicApi.EntityFramework;
using DynamicApi.Services.Listener;

namespace DynamicApi.Services.Handlers;

public class DeletionHandler<T, TDbContext> : EntityStateHandler<T, TDbContext>
    where TDbContext : DynamicContext where T : class {

    public DeletionHandler(ListenerConfiguration configuration, T model, TDbContext context,
        ListenerService<T, TDbContext> service) : base(configuration, model, context, service) {
    }

    public override async Task<Func<Task>> Handle() {
        if(_configuration.OnDeleting) {
            await _service.OnDeleting(_model, _context);
        }

        if(_configuration.OnDeleted) {
            return async () => await _service.OnDeleted(_model, _context);
        }

        return null;
    }

}