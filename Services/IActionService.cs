using DynamicApi.Serializers;

namespace DynamicApi.Services; 

public interface IActionService<in T> : IService{
    
    public Task<object> OnQuery(T input, HttpContext httpContext);
    
    public SerializeType SerializeType { get; } 

}
