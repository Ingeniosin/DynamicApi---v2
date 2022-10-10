using DynamicApi.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace DynamicApi.Routes;

public abstract class StoredRoute<T, TDbContext> : Route where T : class where TDbContext : DynamicContext {
    protected readonly Func<TDbContext, DbSet<T>> DbSet;
    
    protected StoredRoute(string name, Func<TDbContext, DbSet<T>> dbSet) : base(name) {
        DbSet = dbSet;
    }
}