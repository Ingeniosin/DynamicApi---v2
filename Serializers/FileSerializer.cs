namespace DynamicApi.Serializers;

public class FileSerializer : ISerializer {

    public IResult Serialize(object obj) {
        var file = obj as FileInfo;
        return Results.File(file!.OpenRead(), "application/force-download", file.Name, file.LastWriteTime);
    }

}