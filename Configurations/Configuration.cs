using DynamicApi.Routes;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using Route=DynamicApi.Routes.Route;

namespace DynamicApi.Configurations; 

public static class Configuration {
    
    public readonly static List<ServiceInfo> Listeners = new();
    public readonly static Dictionary<Type, Route> Models = new();
    public static IServiceProvider ServiceProvider { get; set; }
    
    public readonly static JsonSerializerSettings JsonBypassConfiguration = new(){
        Formatting = Formatting.None,
        ContractResolver = new CustomContractResolver(){Bypass = true,},
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    };
    
    private readonly static JsonSerializerSettings JsonConfigurations = new(){
        Formatting = Formatting.None,
        ContractResolver = new CustomContractResolver(),
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    };
    
    public static WebApplication Configure<TDbContext>(WebApplicationBuilder builder, List<ServiceInfo> services, List<Action<TDbContext>> defaultValues) where TDbContext : DbContext {
        JsonConvert.DefaultSettings = () => JsonConfigurations;

        services.ForEach(service => {
            if(service.IsScoped) builder.Services.AddScoped(service.ServiceType);
            else builder.Services.AddSingleton(service.ServiceType);
            if(service.IsListener) {
                Listeners.Add(service);
            }
        });
        
        builder.Services.AddControllersWithViews();
        
        builder.Services.Configure<FormOptions>(x => {
            x.ValueLengthLimit = int.MaxValue;
            x.MultipartBodyLengthLimit = int.MaxValue;
            x.MultipartHeadersLengthLimit = int.MaxValue;
        });
        
        builder.Services.Configure<HttpSysOptions>(x => {
            x.MaxRequestBodySize = null;
        });
        
        builder.Services.Configure<KestrelServerOptions>(options => {
            options.Limits.MaxRequestBodySize =  int.MaxValue;
        });
        
        builder.Services.Configure<IISServerOptions>(options => {
            options.MaxRequestBodySize = int.MaxValue;
        });
   
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine($"Connection string: {connectionString}");
        
        builder.Services.AddDbContext<TDbContext>(x => {
            x.UseLazyLoadingProxies()
                .UseNpgsql(connectionString)
                .LogTo(Console.WriteLine, new[] {RelationalEventId.CommandExecuted})
                .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.DetachedLazyLoadingWarning));
        });
        
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var app = builder.Build();

        if (!app.Environment.IsDevelopment()){
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");

        app.MapFallbackToFile("index.html");
        
        ServiceProvider = app.Services;
        
        using var scope = app.Services.CreateScope();
        var applicationDbContext = scope.ServiceProvider.GetService<TDbContext>();
        if (applicationDbContext == null) throw new Exception("ApplicationDbContext is null");
        Console.WriteLine("Starting server...");
        Console.WriteLine("Creando tablas...");
        applicationDbContext.Database.Migrate();
        foreach (var defaultValue in defaultValues) {
            defaultValue(applicationDbContext);
        }
        Console.WriteLine("Tablas creadas correctamente");
        Console.WriteLine($"¡Rutas creadas correctamente!");
        return app;
    }
    
    
}