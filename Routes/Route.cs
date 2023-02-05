namespace DynamicApi.Routes; 

public abstract class Route {
    private string _name;

    public string Name {
        get => "/api/" + _name.Replace("/api/", "");
        set => _name = value;
    }

    protected Route(string name) {
        Name = name;
    }

    public abstract void Load(WebApplication application, ILogger logger);
}