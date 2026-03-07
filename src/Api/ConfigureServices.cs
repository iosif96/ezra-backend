using System.Reflection;

using Api.Filters;
using Api.Hubs;

using Application;
using Application.Common.Interfaces;

using FluentValidation;
using FluentValidation.AspNetCore;

using Hangfire;
using Hangfire.SqlServer;

namespace Api;

public static class ConfigureServices
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
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

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString));
        services.AddHangfireServer();

        services.AddSignalR()
            .AddNewtonsoftJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                options.PayloadSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            });
        services.AddScoped<ITouchpointScanNotifier, TouchpointScanNotifier>();
        services.AddScoped<IOverviewNotifier, OverviewNotifier>();

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
