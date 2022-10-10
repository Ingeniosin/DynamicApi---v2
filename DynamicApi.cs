using DynamicApi.Configurations;
using DynamicApi.EntityFramework;
using DynamicApi.Helpers;
using DynamicApi.Routes;
using Route=DynamicApi.Routes.Route;

namespace DynamicApi; 

public class DynamicApi<TDbContext> where TDbContext : DynamicContext {
    
    private readonly List<Route> _routes;
    private readonly List<ServiceInfo> _services;
    private readonly List<Action<TDbContext>> _defaultValues;
    private readonly WebApplicationBuilder _builder;
    private readonly Action<WebApplication> _preStart;

    public DynamicApi(Action<RouteBuilder<TDbContext>> routeBuilderFn, WebApplicationBuilder builder, Action<WebApplication> preStart = null) {
        var routeBuilder = new RouteBuilder<TDbContext>(_routes = new List<Route>(), _services = new List<ServiceInfo>(), _defaultValues = new List<Action<TDbContext>>(), Configuration.Models);
        routeBuilder.addAction<CreateGridInput, CreateGridAction>("CreateGrid");
        routeBuilder.addAction<CreateFormInput, CreateFormAction>("CreateForm");
        routeBuilderFn(routeBuilder);
        _builder = builder;
        _preStart = preStart;
    }

    public void Start() {
        var app = Configuration.Configure(_builder, _services, _defaultValues);
        _routes.ForEach(route => route.Load(app));
        _preStart?.Invoke(app);
        app.Run();

    }


}