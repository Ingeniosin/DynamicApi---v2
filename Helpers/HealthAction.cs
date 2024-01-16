using DynamicApi.Serializers;
using DynamicApi.Services;

namespace DynamicApi.Helpers;

public class HealthAction : IActionService<object> {

    public Task<object> OnQuery(object input, HttpContext httpContext) {
        return Task.FromResult<object>(new {
            Status = "OK"
        });
    }

    public SerializeType SerializeType => SerializeType.STANDARD;

}