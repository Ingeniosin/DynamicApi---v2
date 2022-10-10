using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.Helpers;

namespace DynamicApi.DevExpress;

public class DataSourceLoadOptions : DataSourceLoadOptionsBase{
    public static ValueTask<DataSourceLoadOptions> BindAsync(HttpContext httpContext){
        var loadOptions = new DataSourceLoadOptions();
        DataSourceLoadOptionsParser.Parse(loadOptions, key => httpContext.Request.Query[key].FirstOrDefault());
        loadOptions.StringToLower = true;
        return ValueTask.FromResult(loadOptions);
    }
}