using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DevExtreme.AspNet.Data;
using DynamicApi.Configurations;
using DynamicApi.DevExpress;
using DynamicApi.EntityFramework;
using DynamicApi.Serializers;
using DynamicApi.Validators;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DynamicApi.Routes.Types; 

public class NonServiceRoutes<T, TDbContext> : StoredRoute<T, TDbContext> where TDbContext : DynamicContext where T : class {

    private readonly IEnumerable<string> _staticFields = typeof(T).GetProperties().Where(x =>  x?.GetGetMethod()?.IsVirtual != true).Select(x => string.Concat(x.Name[..1].ToLower(), x.Name.AsSpan(1))).ToList();
    private readonly IEnumerable<string> _ignoredFields = typeof(T).GetProperties().Where(x => x?.GetCustomAttribute<JsonIgnoreGet>() != null).Select(x => string.Concat(x.Name[..1].ToLower(), x.Name.AsSpan(1))).ToList();
    
    public async Task<IResult> Get(DataSourceLoadOptions loadOptions, TDbContext db) {
        var dbSet = DbSet(db);
        
        var select = loadOptions.Select?.ToList();

        if(select == null) {
            select = _staticFields.ToList();
        } else  {
            var recursiveFields = select.Where(x => x.StartsWith("-")).ToList();
            if(select.Contains("*")) {
                select.AddRange(_staticFields);
                select.Remove("*");
            } 
            select.RemoveAll(x => recursiveFields.Contains(x));
            loadOptions.Select = select.ToArray();
        } 
        
        if(!select.Contains("id")) {
            select.Add("id");
        }
        select.RemoveAll(x => _ignoredFields.Contains(x));
        
        var result = await DataSourceLoader.LoadAsync(dbSet, loadOptions);
        return Serializer.Serialize(result);
    }

    public async Task<IResult> Post(HttpContext context, TDbContext db) {
        var dbSet = DbSet(db);
        var model = dbSet.CreateProxy();
        var values = context.Request.Form["values"];
        JsonConvert.PopulateObject(values, model, Configuration.JsonBypassConfiguration);
        
        ModelValidator.TryValidateModel(model, out var isValid, out var validationResults);

        if(!isValid) {
            return validationResults;
        }
        
        
        await dbSet.AddAsync(model);
        await db.SaveChangesAsync();
        return Serializer.Ok();
    }
    
    public async Task<IResult> Put(HttpContext context, TDbContext db) {
        var dbSet = DbSet(db);
        var key = int.Parse(context.Request.Form["key"].ToString().Replace("\"", ""));
        var values = context.Request.Form["values"];
        var model = await dbSet.FindAsync(key);
        if(model == null)
            throw new Exception("Model not found.");
        JsonConvert.PopulateObject(values, model, Configuration.JsonBypassConfiguration);
        
        ModelValidator.TryValidateModel(model, out var isValid, out var validationResults);

        if(!isValid) {
            return validationResults;
        }
        
        await db.SaveChangesAsync();
        return Serializer.Ok();
    }

    public async Task<IResult> Delete(HttpContext context, TDbContext db) {
        var dbSet = DbSet(db);
        var key = int.Parse(context.Request.Form["key"].ToString().Replace("\"", ""));
        var model = await dbSet.FindAsync(key);
        if(model == null)
            throw new Exception("Model not found.");
        dbSet.Remove(model);
        await db.SaveChangesAsync();
        return Serializer.Ok();
    }

    public override void Load(WebApplication application, ILogger logger) {
        application.MapGet(Name, Get);
        application.MapPost(Name, Post);
        application.MapPut(Name, Put);
        application.MapDelete(Name, Delete);
        logger.LogInformation($"Loaded {Name}");
    }

    public NonServiceRoutes(string name, Func<TDbContext, DbSet<T>> dbSet) : base(name, dbSet) {
    }
}