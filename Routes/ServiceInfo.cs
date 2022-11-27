using DynamicApi.Services.Listener;

namespace DynamicApi.Routes; 

public class ServiceInfo {

    public readonly Type ServiceType;
    public readonly bool IsScoped;
    public readonly ListenerInfo ListenerInfo;
    public bool IsListener => ListenerInfo != null;

    public ServiceInfo(Type serviceType, bool isScoped) {
        ServiceType = serviceType;
        IsScoped = isScoped;
    }

    public ServiceInfo(Type serviceType, bool isScoped, ListenerInfo listenerInfo) : this(serviceType, isScoped) {
        ListenerInfo = listenerInfo;
    }
}

public class ListenerInfo {
    
    public readonly Type ModelType;
    public readonly string TypeModelName;
    public readonly ListenerConfiguration Configuration;

    public ListenerInfo(Type modelType, ListenerConfiguration configuration) {
        ModelType = modelType;
        Configuration = configuration;
        TypeModelName = modelType.FullName;
    }

}