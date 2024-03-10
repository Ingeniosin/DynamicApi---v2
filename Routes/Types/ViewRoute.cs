using System.Reflection;
using DynamicApi.Configurations;
using DynamicApi.DevExpress;

namespace DynamicApi.Routes.Types;

public class ViewRoute<T> : Route where T : class {
    
    public readonly IEnumerable<string> _ignoredFields = typeof(T).GetProperties()
        .Where(x => x?.GetCustomAttribute<JsonIgnoreGet>() != null)
        .Select(x => string.Concat(x.Name[..1].ToLower(), x.Name.AsSpan(1))).ToList();

    public readonly IEnumerable<string> _staticFields = typeof(T).GetProperties()
        .Where(x => x?.GetGetMethod()?.IsVirtual != true)
        .Select(x => string.Concat(x.Name[..1].ToLower(), x.Name.AsSpan(1))).ToList();
    
    public Type Type { get; }

    public ViewRoute(string name, Type type) : base("view/" + name) {
        Type = type;
    }

    public override void Load(WebApplication application, ILogger logger) {
        application.MapGet(Name, async (DataSourceLoadOptions loadOptions, HttpContext context) => {
            using var scope = application.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService(Type) as ViewRouteImpl;

            if(service == null) {
                throw new Exception("Could not find service");
            }
            
            var select = loadOptions.Select?.ToList();

            if(select == null) {
                select = _staticFields.ToList();
            }
            else {
                var recursiveFields = select.Where(x => x.StartsWith("-")).ToList();

                if(select.Contains("*")) {
                    select.AddRange(_staticFields);
                    select.Remove("*");
                }

                select.RemoveAll(x => recursiveFields.Contains(x));
                loadOptions.Select = select.ToArray();
            }

            if(!select.Contains("id")) {
                select.Add("id");
            }
            
            select.RemoveAll(x => _ignoredFields.Contains(x));

            return await service.Get(loadOptions, context);
        });
        logger.LogInformation($"Loaded {Name}");
    }

}