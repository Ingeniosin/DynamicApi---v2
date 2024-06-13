using DynamicApi.Configurations;
using DynamicApi.EntityFramework;
using DynamicApi.Exceptions;
using DynamicApi.Helpers;
using DynamicApi.Routes;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using NLog.Extensions.Logging;
using Route = DynamicApi.Routes.Route;

namespace DynamicApi;

public class DynamicApi<TDbContext> where TDbContext : DynamicContext {

    private readonly List<Route> _routes;
    private readonly List<ServiceInfo> _services;
    private readonly List<Action<TDbContext>> _defaultValues;
    private readonly WebApplicationBuilder _builder;
    private readonly Action<WebApplication, ILogger<DynamicApi<TDbContext>>> _preStart;
    private ILogger<DynamicApi<TDbContext>> _logger;


    public DynamicApi(Action<RouteBuilder<TDbContext>> routeBuilderFn, WebApplicationBuilder builder,
        Action<WebApplication, ILogger<DynamicApi<TDbContext>>> preStart = null) {
        var nlog = File.Exists("nlog.config");

        using var loggerFactory = LoggerFactory.Create(loggingBuilder => {
            loggingBuilder.SetMinimumLevel(LogLevel.Information).AddConsole();

            if(nlog) {
                loggingBuilder.SetMinimumLevel(LogLevel.Information).AddNLog();
            }
        });

        _logger = loggerFactory.CreateLogger<DynamicApi<TDbContext>>();

        if(nlog) {
            _logger.LogInformation("NLog enabled");
        }

        var routeBuilder = new RouteBuilder<TDbContext>(
            _routes = new List<Route>(),
            _services = new List<ServiceInfo>(),
            _defaultValues = new List<Action<TDbContext>>(),
            Configuration.Models
        );

        if(builder.Environment.IsDevelopment()) {
            routeBuilder.addAction<CreateGridInput, CreateGridAction>("CreateGrid");
            routeBuilder.addAction<CreateFormInput, CreateFormAction>("CreateForm");
            _logger.LogWarning("Development mode enabled, adding CreateGrid and CreateForm actions");
        }

        routeBuilder.addAction<object, HealthAction>("Health");

        routeBuilderFn(routeBuilder);
        _builder = builder;
        _preStart = preStart;
    }

    public void Start() {
        var app = Configuration.Configure(_builder, _services, _defaultValues, _logger);



        var staticPath = Path.Combine(_builder.Environment.ContentRootPath, "Static");
        
        bool exists = Directory.Exists(staticPath);
        if(exists) {
            _logger.LogInformation("[class:DynamicApi][method:Start] Static directory found, serving static files: {staticPath}", staticPath);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    staticPath),
                RequestPath = "/static"
            });
        }
        
        
        app.UseExceptionHandler(c => c.Run(async context => {
            Exception exception = context.Features.Get<IExceptionHandlerPathFeature>().Error;
            if(exception is ApiException apiException) {
                context.Response.StatusCode = (int) apiException.StatusCode;
            }

            //has header x-debug
            var bypass = context.Request.Headers["x-debug"].Any();
            var isDevelopment = _builder.Environment.IsDevelopment() || bypass;
            var response = new {
                error = exception.Message,
                detail =  isDevelopment ? exception?.InnerException?.Message : null,
                stack = isDevelopment ? exception.StackTrace : null,
            };
            await context.Response.WriteAsJsonAsync(response);
        }));
        
        _routes.ForEach(route => route.Load(app, _logger));
        _preStart?.Invoke(app, _logger);
        app.Run();
    }

}