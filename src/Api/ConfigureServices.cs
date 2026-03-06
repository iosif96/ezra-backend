using System.Reflection;

using Api.Filters;

using Application;

using FluentValidation;
using FluentValidation.AspNetCore;

namespace Api;

public static class ConfigureServices
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                // This tells Newtonsoft.Json:
                // 1. If DateTime.Kind is Unspecified, assume it's UTC.
                // 2. If DateTime.Kind is Local, convert it to UTC before serializing.
                // 3. If DateTime.Kind is Utc, serialize as UTC.
                // It will append 'Z' for UTC times.
            });
        services.AddEndpointsApiExplorer();

        services.AddAuthentication();
        services.AddAuthorization();

        // Extension classes
        services.AddHttpClient();
        services.AddHealthChecks()
            .AddDbContextCheck<Application.Infrastructure.Persistence.ApplicationDbContext>();

        services.AddCorsCustom();
        services.AddJWTCustom(configuration);
        services.AddHttpContextAccessor();

        services.AddSingleton<ApiExceptionFilterAttribute>();

        // CORS
        services.AddCors(options => options.AddPolicy(
                "MyAllowedOrigins",
                policy => policy
                    .WithOrigins(
                "http://localhost:8100",
                "http://localhost:8101",
                "http://localhost:60976",
                "https://localhost:8100",
                "http://192.168.1.207:8100",
                "https://localhost",
                "http://localhost",
                "http://localhost:8080",
                "http://localhost:4200",
                "capacitor://localhost",
                "ionic://localhost",
                "https://192.168.1.216:8100",
                "https://city-alert-api.azurewebsites.net")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

        return services;
    }
}
