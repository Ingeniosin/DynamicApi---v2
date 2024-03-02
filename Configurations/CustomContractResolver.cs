using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DynamicApi.Configurations;

public class CustomContractResolver : DefaultContractResolver {

    public bool Bypass = false;

    public CustomContractResolver() {
        NamingStrategy = new CamelCaseNamingStrategy();
    }


    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
        var property = base.CreateProperty(member, memberSerialization);

        if(Bypass) {
            return property;
        }

        var attributes = property?.AttributeProvider?.GetAttributes(true);
        var nameLower = property?.PropertyName?.ToLower() ?? string.Empty;

        if(nameLower.Equals("id") || attributes?.Contains(new JsonShow()) == true) {
            return property;
        }

        var hasIgnoreGet = attributes?.Contains(new JsonIgnoreGet()) == true;
        property.Readable = !hasIgnoreGet && !IsVirtual(member);
        return property;
    }

    private static bool IsVirtual(MemberInfo member) {
        dynamic method = member.GetType().GetProperty("GetMethod")?.GetValue(member, null);
        return method?.IsVirtual == true;
    }

}

public class JsonIgnoreGet : Attribute {

}

public class JsonShow : Attribute {

}