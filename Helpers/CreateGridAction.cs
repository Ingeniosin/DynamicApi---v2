using System.Reflection;
using DynamicApi.Configurations;
using DynamicApi.Serializers;
using DynamicApi.Services;
using Panic.StringUtils.Extensions;

namespace DynamicApi.Helpers; 

public class CreateGridAction : IActionService<CreateGridInput> {

    private readonly string TemplateGrid =  File.ReadAllText(@"D:\Escritorio\Lib\DynamicApi\Helpers\TemplateGrid.txt");
    private readonly string TemplateDependencyGrid =  File.ReadAllText(@"D:\Escritorio\Lib\DynamicApi\Helpers\TemplateDependencyGrid.txt");

    public async Task<object> OnQuery(CreateGridInput input, HttpContext httpContext) {
        var inputModel = input.Class.ToLower().Trim();
        var models = Configuration.Models.Keys.ToList();
        var model = models.FirstOrDefault(x => x.Name.ToLower().Trim().Equals(inputModel));
        model ??= models.First(x => x.Name.ToLower().Trim().StartsWith(inputModel));
        
        
        var properties = model.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => !(x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
            .Where(x => !(x.PropertyType== typeof(int) && x.Name.EndsWith("Id"))).ToList();

        var notDependency = string.IsNullOrEmpty(input.Dependency);
        var dependency = properties.FirstOrDefault(x => x.Name.ToLower().Replace("Id", "").Equals(input.Dependency.ToLower().Replace("Id", "").Trim()));
        if(!notDependency && dependency == null) {
            notDependency = true;
        } else {
            properties.Remove(dependency);
        }
        var dependencyName = (dependency?.Name ?? string.Empty) + "Id";
        
        
        var captions = CreatorService.BuildCaptions(properties);
        var columns = BuildColumns(properties, models);
        var api = Configuration.Models[model].Name.Replace("/api/", "");

      
        
        var placeholders = new Dictionary<string, string>(){
            { "captions", captions },
            { "columns", columns },
            { "api", api },
            { "model", inputModel },
            {"campoCamel", dependencyName.ToCamelCase()},
            {"campo", dependencyName}
        };

        return await CreatorService.BuildTempFile(notDependency ? TemplateGrid : TemplateDependencyGrid, placeholders);
    }

   

    private static string BuildColumns(IEnumerable<PropertyInfo> properties, IReadOnlyCollection<Type> models) {
        return string.Join(", \n\t", properties.Select(x => {
            var dataField = x.Name.ToCamelCase();
            var isBoolean = x.PropertyType == typeof(bool);
            var isNullable = x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && !isBoolean;
            var nullableStr = (!isNullable).ToString().ToLower();
            var type = CreatorService.GetGridType(x);
            var isVirtual = x.GetGetMethod()!.IsVirtual;
            var lookup = "";
            if(isVirtual) {
                var relationRoute = models.FirstOrDefault(y => y.Name == x.PropertyType.Name);
                if(relationRoute != null) {
                    var relName = Configuration.Models[relationRoute].Name.Replace("/api/", "");
                    var display = relationRoute.GetProperties().FirstOrDefault(y => y.PropertyType == typeof(string))?.Name ?? "Undefined";
          
                    lookup = $", lookup: getDsLookup('{relName}', null, '{display.ToCamelCase()}')";
                    dataField += "Id";
                    type = "number";
                 
                } else {
                    Console.WriteLine($"No se encontro la ruta para el modelo {x.PropertyType.Name}");
                }
             
            }
            
            return $"{{dataField: '{dataField}', dataType: '{type}', caption: captions['{x.Name.ToCamelCase()}'], required: {nullableStr}{lookup}}}";
        }));
    }



    public SerializeType SerializeType => SerializeType.FILE;
}

public class CreateGridInput {
    public string Class { get; set; }
    public string Dependency { get; set; } = "";
}
