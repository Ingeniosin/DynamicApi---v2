using DynamicApi.EntityFramework;
using DynamicApi.Services.Listener;

namespace DynamicApi.Services.Handlers;

public class EntityStateHandler<T, TDbContext> where TDbContext : DynamicContext where T : class {

    protected readonly ListenerConfiguration _configuration;
    protected readonly ListenerService<T, TDbContext> _service;
    protected readonly T _model;
    protected readonly TDbContext _context;

    public EntityStateHandler(ListenerConfiguration configuration, T model, TDbContext context, ListenerService<T, TDbContext> service) {
        _configuration = configuration;
        _model = model;
        _context = context;
        _service = service;
    }

    public virtual Task<Func<Task>> Handle() => Task.FromResult<Func<Task>>(null);

}