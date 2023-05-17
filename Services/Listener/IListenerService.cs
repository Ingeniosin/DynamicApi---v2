using DynamicApi.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace DynamicApi.Services.Listener; 

public interface IListenerService{
    Task<Func<Task>> Handle(object model, ListenerConfiguration configuration, EntityState state, DynamicContext context);
}