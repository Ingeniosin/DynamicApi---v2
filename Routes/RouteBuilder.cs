using System.Reflection;
using DynamicApi.EntityFramework;
using DynamicApi.Routes.Types;
using DynamicApi.Services;
using DynamicApi.Services.Listener;
using Microsoft.EntityFrameworkCore;

namespace DynamicApi.Routes; 

public class RouteBuilder<TDbContext> where TDbContext : DynamicContext {

    private readonly List<Route> _routes;
    private readonly List<ServiceInfo> _services;
    private readonly Dictionary<Type, Route>_models;
    private readonly List<Action<TDbContext>> _defaultValues;
    

    public RouteBuilder(List<Route> routes, List<ServiceInfo> services, List<Action<TDbContext>> defaultValues, Dictionary<Type, Route> models) {
        _routes = routes;
        _services = services;
        _defaultValues = defaultValues;
        _models = models;
    }

    public RouteBuilder<TDbContext> addNonService<T>(Func<TDbContext, DbSet<T>> dbSet) where T : class {
        var propertyInfo = typeof(TDbContext).GetProperties().FirstOrDefault(p => p.PropertyType == typeof(DbSet<T>));
        var name = propertyInfo?.Name;
        if (name == null)
            throw new Exception("Could not find property name");
        addNonService(name, dbSet);
        return this;
    }

    public RouteBuilder<TDbContext> addNonService<T>(string name, Func<TDbContext, DbSet<T>> dbSet) where T : class {
        var route = new NonServiceRoutes<T, TDbContext>(name, dbSet);
        _routes.Add(route);
        _models.Add(typeof(T), route);
        CheckDefaultValues(dbSet);
        return this;
    }

    private void CheckDefaultValues<T>(Func<TDbContext, DbSet<T>> dbSet) where T : class {
        var defaultValuesFn = typeof(T).GetMethod("DefaultValues", BindingFlags.Public | BindingFlags.Static);
        if(defaultValuesFn != null) {
            _defaultValues.Add(context => {
                var set = dbSet(context);
                if(set.Any()) return;
                var defaultValues = defaultValuesFn.Invoke(null, new object[] { context }) as List<T>;
                set.AddRange(defaultValues!);
                context.SaveChanges();
            });
        }
    }

    public RouteBuilder<TDbContext> addService<T, TService>(string name, Func<TDbContext, DbSet<T>> dbSet, bool isScoped = false) where T : class where TService : ListenerService<T, TDbContext> {
        var serviceType = typeof(TService);
        var configuration = serviceType.GetProperty("Configuration", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as ListenerConfiguration;
        if(configuration == null)
            throw new Exception("Could not find configuration");
        var listenerInfo = new ListenerInfo(typeof(T), configuration);
        var route = new NonServiceRoutes<T, TDbContext>(name, dbSet);
        _routes.Add(route);
        _services.Add(new ServiceInfo(serviceType, isScoped, listenerInfo));
        _models.Add(typeof(T), route);
        CheckDefaultValues(dbSet);
        return this;
    }

    public RouteBuilder<TDbContext> addService<T, TService>(Func<TDbContext, DbSet<T>> dbSet, bool isScoped = false) where T : class where TService : ListenerService<T, TDbContext> {
        var propertyInfo = typeof(TDbContext).GetProperties().FirstOrDefault(p => p.PropertyType == typeof(DbSet<T>));
        var name = propertyInfo?.Name;
        if (name == null)
            throw new Exception("Could not find property name");
        addService<T, TService>(name, dbSet, isScoped);
        return this;
    }

    public RouteBuilder<TDbContext> addAction<TIn, TService>(string name, bool isScoped = false) where TIn : class where TService : IActionService<TIn> {
        var route = new ActionRoute<TIn, TService>(name);
        _routes.Add(route);
        _services.Add(new ServiceInfo(typeof(TService), isScoped));
        _models.Add(typeof(TIn), route);
        return this;
    }
    
}