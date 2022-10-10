using System.Reflection;
using DynamicApi.Configurations;
using DynamicApi.Serializers;
using DynamicApi.Services;
using Panic.StringUtils.Extensions;

namespace DynamicApi.Helpers; 

public class CreateGridAction : IActionService<CreateGridInput> {

    private readonly string TemplateGrid =  File.ReadAllText(@"D:\Escritorio\Lib\DynamicApi\Helpers\TemplateGrid.txt");

    public async Task<object> OnQuery(CreateGridInput input, HttpContext httpContext) {
        var inputModel = input.Class.ToLower().Trim();
        var models = Configuration.Models.Keys.ToList();
        var model = models.First(x => x.Name.ToLower().Trim() == inputModel);
        var properties = model.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.PropertyType != typeof(int) && !x.Name.EndsWith("Id")).ToList();

        var captions = CreatorService.BuildCaptions(properties);
        var columns = BuildColumns(properties, models);
        var api = Configuration.Models[model].Name.Replace("/api/", "");
        
        var placeholders = new Dictionary<string, string>(){
            { "captions", captions },
            { "columns", columns },
            { "api", api },
            { "model", inputModel }
        };

        return await CreatorService.BuildTempFile(TemplateGrid, placeholders);
    }

   

    private static string BuildColumns(IEnumerable<PropertyInfo> properties, IReadOnlyCollection<Type> models) {
        return string.Join(", \n\t", properties.Select(x => {
            var dataField = x.Name.ToCamelCase();
            var isNullable = x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
            var nullableStr = (!isNullable).ToString().ToLower();
            var type = CreatorService.GetGridType(x);
            var isVirtual = x.GetGetMethod()!.IsVirtual;
            var lookup = "";
            if(isVirtual) {
                var relationRoute = models.First(y => y.Name == x.PropertyType.Name);
                var relName = Configuration.Models[relationRoute].Name.Replace("/api/", "");
                var display = relationRoute.GetProperties().First(y => y.PropertyType == typeof(string)).Name;
                lookup = $", lookup: getDsLookup('{relName}', null, '{display.ToCamelCase()}')";
                dataField += "Id";
                type = "number";
            }
            
            return $"{{dataField: '{dataField}', dataType: '{type}', caption: captions['{x.Name.ToCamelCase()}'], required: {nullableStr}{lookup}}}";
        }));
    }



    public SerializeType SerializeType => SerializeType.FILE;
}

public class CreateGridInput {
    public string Class { get; set; }
}
