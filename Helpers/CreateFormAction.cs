using System.Reflection;
using DynamicApi.Configurations;
using DynamicApi.Serializers;
using DynamicApi.Services;
using Panic.StringUtils.Extensions;

namespace DynamicApi.Helpers;

public class CreateFormAction : IActionService<CreateFormInput> {

    private readonly string TemplateForm = File.ReadAllText(@"E:\Escritorio\Lib\DynamicApi\Helpers\TemplateForm.txt");


    public async Task<object> OnQuery(CreateFormInput input, HttpContext httpContext) {
        var inputModel = input.Class.ToLower().Trim();
        var models = Configuration.Models.Keys.ToList();
        var model = models.First(x => x.Name.ToLower().Trim() == inputModel);
        var properties = model.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.PropertyType != typeof(int) && !x.Name.EndsWith("Id")).ToList();

        var captions = CreatorService.BuildCaptions(properties);
        var items = BuildItems(properties, models);
        var api = Configuration.Models[model].Name.Replace("/api/", "");
        var labels = CreatorService.BuildLabels(properties);


        var placeholders = new Dictionary<string, string>() {
            { "captions", captions },
            { "items", items },
            { "api", api },
            { "model", inputModel },
            { "labels", labels }
        };

        return await CreatorService.BuildTempFile(TemplateForm, placeholders);
    }


    private static string BuildItems(IEnumerable<PropertyInfo> properties, IReadOnlyCollection<Type> models) {
        var lines = properties.Select(x => {
            var dataField = x.Name.ToCamelCase();
            var isNullable = x.PropertyType.IsGenericType &&
                             x.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
            var isVirtual = x.GetGetMethod()!.IsVirtual;
            var dxType = CreatorService.GetEditorType(x);
            var isBoolean = x.PropertyType.Name == "Boolean";
            var options = "";

            if(isVirtual) {
                var relationRoute = models.FirstOrDefault(y => y.Name == x.PropertyType.Name);

                if(relationRoute != null) {
                    var relName = Configuration.Models[relationRoute].Name.Replace("/api/", "");
                    var display = relationRoute.GetProperties().First(y => y.PropertyType == typeof(string)).Name;
                    dataField += "Id";
                    dxType = "dxSelectBox";
                    options = $" editorOptions={{getDsLookupForm('{relName}', null, '{display}')}}";
                }
                else {
                    Console.WriteLine($"No se encontro la ruta para el modelo {x.PropertyType.Name}");
                }

            }

            var result = new List<string>();
            result.Add(
                $"<SimpleItem dataField='{dataField}' label={{labels['{x.Name.ToCamelCase()}']}} editorType='{dxType}'{options}>");

            if(!isNullable && !isBoolean) {
                result.Add("\t<RequiredRule message={captions['required']} />");
            }

            result.Add("</SimpleItem>");
            return result;
        }).Aggregate(new List<string>(), (current, next) => {
            current.AddRange(next);
            return current;
        });
        return string.Join("\n\t\t\t", lines);
    }

    public SerializeType SerializeType => SerializeType.FILE;

}

public class CreateFormInput {

    public string Class { get; set; }

}