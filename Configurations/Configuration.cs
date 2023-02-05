using System.IO.Compression;
using DynamicApi.Routes;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
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
    
    public static WebApplication Configure<TDbContext>(WebApplicationBuilder builder, List<ServiceInfo> services, List<Action<TDbContext>> defaultValues, ILogger logger) where TDbContext : DbContext {
        JsonConvert.DefaultSettings = () => JsonConfigurations;
        
        services.ForEach(service => {
            if(service.IsScoped) 
                builder.Services.AddScoped(service.ServiceType);
            else 
                builder.Services.AddSingleton(service.ServiceType);
            if(service.IsListener) {
                Listeners.Add(service);
            }
        });
        
        logger.LogInformation("Configuring services...");
        
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
        
        builder.Services.AddCors(options => {
            options.AddPolicy("AllowAll", corsPolicyBuilder => {
                corsPolicyBuilder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        
        
        builder.Services.Configure<GzipCompressionProviderOptions>(options => {
            options.Level = CompressionLevel.SmallestSize;
        });
        builder.Services.Configure<BrotliCompressionProviderOptions>(options => {
            options.Level = CompressionLevel.SmallestSize;
        });
        
        builder.Services.AddResponseCompression(options => {
            options.Providers.Add<GzipCompressionProvider>();
            options.Providers.Add<BrotliCompressionProvider>();
            
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {
                "application/octet-stream",
                "application/json",
                "application/xml",
                "text/plain",
                "text/xml",
                "text/json",
            });
            options.EnableForHttps = true;

        });

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        logger.LogWarning($"Connection string: {connectionString}");
        
        builder.Services.AddDbContext<TDbContext>(x => {
            x.UseLazyLoadingProxies()
                .UseNpgsql(connectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.DetachedLazyLoadingWarning));

            if(builder.Environment.IsDevelopment()) {
                x.LogTo(msg => logger.LogInformation(msg), new[] {RelationalEventId.CommandExecuted});
            }

        });
        
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var app = builder.Build();
        app.UseResponseCompression();

        if (!app.Environment.IsDevelopment()){
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        
        app.UseCors("AllowAll");

        app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");

        app.MapFallbackToFile("index.html");
        
        ServiceProvider = app.Services;
        
        using var scope = app.Services.CreateScope();
        var applicationDbContext = scope.ServiceProvider.GetService<TDbContext>();
        if (applicationDbContext == null) throw new Exception("ApplicationDbContext is null");
        logger.LogInformation("Starting server...");
        applicationDbContext.Database.Migrate();
        foreach (var defaultValue in defaultValues) {
            defaultValue(applicationDbContext);
        }
        applicationDbContext.Database.ExecuteSqlRaw("SET TIME ZONE 'America/Bogota';");
        var timezone = applicationDbContext.Database.ExecuteSqlRaw("SELECT current_setting('TIMEZONE');");
        logger.LogWarning($"Timezone DB: {timezone}");
        logger.LogWarning($"Timezone .net: {TimeZoneInfo.Local.DisplayName}");
        logger.LogInformation("Tablas creadas!");

        return app;
    }
    
    
}