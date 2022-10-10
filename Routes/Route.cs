namespace DynamicApi.Routes; 

public abstract class Route {
    public readonly string Name;

    protected Route(string name) {
        Name = "/api/"+name;
    }

    public abstract void Load(WebApplication application);
}