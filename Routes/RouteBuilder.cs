using System.Reflection;
using DynamicApi.EntityFramework;
using DynamicApi.Routes.Types;
using DynamicApi.Services;
using DynamicApi.Services.Listener;
using Microsoft.EntityFrameworkCore;

namespace DynamicApi.Routes;

public class RouteBuilder<TDbContext> where TDbContext : DynamicContext {

    private readonly List<Action<TDbContext>> _defaultValues;
    private readonly Dictionary<Type, Route> _models;

    private readonly List<Route> _routes;
    private readonly List<ServiceInfo> _services;
    private string _prefix;

    public RouteBuilder(List<Route> routes, List<ServiceInfo> services, List<Action<TDbContext>> defaultValues,
        Dictionary<Type, Route> models) {
        _routes = routes;
        _services = services;
        _defaultValues = defaultValues;
        _models = models;
    }

    public RouteBuilder<TDbContext> Prefix(string prefix) {
        _prefix = prefix;
        return this;
    }

    public RouteBuilder<TDbContext> addNonService<T>(Func<TDbContext, DbSet<T>> dbSet) where T : class {
        var propertyInfo = typeof(TDbContext).GetProperties().FirstOrDefault(p => p.PropertyType == typeof(DbSet<T>));
        var name = propertyInfo?.Name;

        if(name == null) {
            throw new Exception("Could not find property name");
        }

        addNonService(name, dbSet);
        return this;
    }

    public void addRoute(Route route) {
        route.Name = _prefix + route.Name;
        _routes.Add(route);
    }

    public RouteBuilder<TDbContext> addNonService<T>(string name, Func<TDbContext, DbSet<T>> dbSet) where T : class {
        var route = new NonServiceRoutes<T, TDbContext>(name, dbSet);
        addRoute(route);
        _models.Add(typeof(T), route);
        CheckDefaultValues(dbSet);
        return this;
    }

    public RouteBuilder<TDbContext> addEnum<T>(string name) where T : struct, Enum {
        var route = new EnumRoute<T>(name);
        addRoute(route);
        return this;
    }

    public RouteBuilder<TDbContext> addEnum<T>() where T : struct, Enum {
        var route = new EnumRoute<T>(typeof(T).Name);
        addRoute(route);
        return this;
    }

    public RouteBuilder<TDbContext> addView<T, TService>(string name, bool isScoped) where T : class where TService : ViewRouteImpl {
        var serviceType = typeof(TService);
        addRoute(new ViewRoute<T>(name, serviceType));
        _services.Add(new ServiceInfo(serviceType, isScoped));
        return this;
    }

    private void CheckDefaultValues<T>(Func<TDbContext, DbSet<T>> dbSet) where T : class {
        var defaultValuesFn = typeof(T).GetMethod("DefaultValues", BindingFlags.Public | BindingFlags.Static);

        if(defaultValuesFn != null) {
            _defaultValues.Add(context => {
                var set = dbSet(context);

                if(set.Any()) {
                    return;
                }

                var defaultValues = defaultValuesFn.Invoke(null, new object[] { context }) as List<T>;
                set.AddRange(defaultValues!);
                context.SaveChanges();
            });
        }
    }

    public RouteBuilder<TDbContext> addService<T, TService>(string name, Func<TDbContext, DbSet<T>> dbSet,
        bool isScoped = false) where T : class where TService : ListenerService<T, TDbContext> {
        var serviceType = typeof(TService);
        var configuration =
            serviceType.GetProperty("Configuration", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as
                ListenerConfiguration;

        if(configuration == null) {
            throw new Exception(
                "Could not find configuration, in order to use this method you must have a static property called Configuration in your service");
        }

        var listenerInfo = new ListenerInfo(typeof(T), configuration);
        var route = new NonServiceRoutes<T, TDbContext>(name, dbSet);
        addRoute(route);
        _services.Add(new ServiceInfo(serviceType, isScoped, listenerInfo));
        _models.Add(typeof(T), route);
        CheckDefaultValues(dbSet);
        return this;
    }

    public RouteBuilder<TDbContext> addGeneral<T, TService>(bool isScoped = false)
        where T : class where TService : ListenerService<T, TDbContext> {
        var serviceType = typeof(TService);
        var configuration =
            serviceType.GetProperty("Configuration", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as
                ListenerConfiguration;

        if(configuration == null) {
            throw new Exception("Could not find configuration");
        }

        var listenerInfo = new ListenerInfo(typeof(T), configuration);
        /*var route = new NonServiceRoutes<T, TDbContext>(name, dbSet);
          addRoute(route);*/
        _services.Add(new ServiceInfo(serviceType, isScoped, listenerInfo));
        //_models.Add(typeof(T), route);
        //CheckDefaultValues(dbSet);
        return this;
    }

    public RouteBuilder<TDbContext> addService<T, TService>(Func<TDbContext, DbSet<T>> dbSet, bool isScoped = false)
        where T : class where TService : ListenerService<T, TDbContext> {
        var propertyInfo = typeof(TDbContext).GetProperties().FirstOrDefault(p => p.PropertyType == typeof(DbSet<T>));
        var name = propertyInfo?.Name;

        if(name == null) {
            throw new Exception("Could not find property name");
        }

        addService<T, TService>(name, dbSet, isScoped);
        return this;
    }

    public RouteBuilder<TDbContext> addAction<TIn, TService>(string name, bool isScoped = false)
        where TIn : class where TService : IActionService<TIn> {
        var route = new ActionRoute<TIn, TService>(name);
        addRoute(route);
        _services.Add(new ServiceInfo(typeof(TService), isScoped));
        _models.Add(typeof(TIn), route);
        return this;
    }

}