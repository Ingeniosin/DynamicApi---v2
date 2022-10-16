using DynamicApi.Serializers;
using DynamicApi.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DynamicApi.Routes.Types; 

public class ActionRoute<TIn, TService> : Route where TService : IActionService<TIn> {
    private static async Task<IResult> Post(HttpContext httpContext, [FromServices] TService service) {
        var values = httpContext.Request.Form["values"];
        /*var contentType = httpContext.Request.ContentType;
        string values;
        if(contentType == "application/json") {
            var requestBody = httpContext.Request.Body;
            var reader = new StreamReader(requestBody);
            values = await reader.ReadToEndAsync();
        } else {
            values = httpContext.Request.Form["values"];
        }*/

        var newInstance = string.IsNullOrEmpty(values) ? Activator.CreateInstance<TIn>() : JsonConvert.DeserializeObject<TIn>(values);
        try {
            var result = await service.OnQuery(newInstance, httpContext);
            return Serializer.Serialize(result, service.SerializeType);
        } catch(Exception e) {
            Console.WriteLine(e);
            return Results.BadRequest(e);
        }
    }

    public ActionRoute(string name) : base(name) {
    }

    public override void Load(WebApplication application) {
        application.MapPost(Name, Post);
    }
}