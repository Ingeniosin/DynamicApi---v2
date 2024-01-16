using DynamicApi.DevExpress;

namespace DynamicApi.Routes.Types;

public abstract class ViewRouteImpl {

    public abstract Task<IResult> Get(DataSourceLoadOptions loadOptions, HttpContext context);

}