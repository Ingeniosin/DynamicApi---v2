namespace DynamicApi.Services.Listener; 

public class ListenerConfiguration {
    public bool OnCreating { get; set; }
    public bool OnCreated { get; set; }

    public bool OnUpdating { get; set; }
    public bool OnUpdated { get; set; }

    public bool OnDeleting { get; set; }
    public bool OnDeleted { get; set; }
}