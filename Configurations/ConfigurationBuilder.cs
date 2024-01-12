using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DynamicApi.Configurations;

public class ConfigurationBuilder {

    [Required(ErrorMessage = "The connection string is required, please use 'SetConnectionString' method.")]
    public string ConnectionString { get; set; }
    
    [Required(ErrorMessage = "The database provider is required, please use 'SetDatabaseProvider' method.")]
    public Action<DbContextOptionsBuilder> DbContextOptionsBuilder { get; set; }

    public void LoadDefaults(WebApplicationBuilder builder) {
        if(string.IsNullOrEmpty(ConnectionString)) {
            ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            var databaseHost = Environment.GetEnvironmentVariable("DATABASE_URL");
            var databaseName = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "postgres";
            var databaseUser = Environment.GetEnvironmentVariable("DATABASE_USER") ?? "postgres";
            var databasePassword = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
            var databasePort = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";

            if(databaseHost != null && databasePassword != null) {
                ConnectionString = $"User ID={databaseUser};Password={databasePassword};Host={databaseHost};Port={databasePort};Database={databaseName};Pooling=true;";
            }
        }
        
        if(DbContextOptionsBuilder == null) {
            DbContextOptionsBuilder = x => {
                x.UseLazyLoadingProxies().UseNpgsql(ConnectionString);
            };
        }
        
    }
    
    public void Build() {
        Validate();
    }

    private void Validate() {
        var context = new ValidationContext(this, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(this, context, results, true);

        if(isValid) {
            return;
        }

        var message = results.Aggregate("Error validating ConfigurationBuilder: ", (current, validationResult) => current + validationResult.ErrorMessage + " | ");
        throw new Exception(message);
    }
}