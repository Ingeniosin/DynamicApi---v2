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

    private async Task<List<Func<Task>>> OnSaving(AsyncServiceScope scope) {
        return await new EntityHandler(scope, ChangeTracker, this).OnSaving();
    }
}

public static class DynamicExtensions {
    public static IQueryable<object> Set (this DbContext _context, Type t) {
        return (IQueryable<object>)_context.GetType().GetMethod("Set").MakeGenericMethod(t).Invoke(_context, null);
    }
}