using System.Reflection;
using Panic.StringUtils.Extensions;

namespace DynamicApi.Helpers;

public class CreatorService {

    private const string Temp = @"D:\Escritorio\Lib\DynamicApi\Temp\Grid.js";


    public static string BuildCaptions(IEnumerable<PropertyInfo> properties) {
        return string.Join(", \n\t",
            properties.Select(x => $"{x.Name.ToCamelCase()}: '{string.Join(" ", x.Name.ToWords())}'"));
    }

    public static string BuildLabels(IEnumerable<PropertyInfo> properties) {
        return string.Join(", \n\t", properties.Select(x => {
            var camelCase = x.Name.ToCamelCase();
            var isBoolean = x.PropertyType == typeof(bool);
            return isBoolean
                ? $"{camelCase}: {{text: captions['{camelCase}'],  alignment: 'center', location: 'left',}}"
                : $"{camelCase}: {{text: captions['{camelCase}']}}";
        }));
    }

    public static string GetGridType(PropertyInfo x) {
        return x.PropertyType.Name switch {
            "String" => "string",
            "Int32" => "number",
            "DateTime" => "datetime",
            "Boolean" => "boolean",
            _ => "object"
        };
    }

    // 'dxAutocomplete' | 'dxCalendar' | 'dxCheckBox' | 'dxColorBox' | 'dxDateBox' | 'dxDropDownBox' | 'dxHtmlEditor' | 'dxLookup' | 'dxNumberBox' | 'dxRadioGroup' | 'dxRangeSlider' | 'dxSelectBox' | 'dxSlider' | 'dxSwitch' | 'dxTagBox' | 'dxTextArea' | 'dxTextBox'
    public static string GetEditorType(PropertyInfo x) {
        return x.PropertyType.Name switch {
            "String" => "dxTextBox",
            "Int32" => "dxNumberBox",
            "DateTime" => "dxDateBox",
            "Boolean" => "dxSwitch",
            _ => "dxTextBox"
        };
    }

    public static async Task<FileInfo> BuildTempFile(string template, Dictionary<string, string> placeholders) {
        var result = placeholders.Aggregate(template,
            (current, registry) => current.Replace($"%{registry.Key}%", registry.Value));
        new FileInfo(Temp).Directory?.Create();
        await File.WriteAllTextAsync(Temp, result);
        return new FileInfo(Temp);
    }

}