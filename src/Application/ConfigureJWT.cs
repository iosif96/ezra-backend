using System.Text;

using Ardalis.GuardClauses;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Application;

public static class ConfigureJWT
{
    public static IServiceCollection AddJWTCustom(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfiguration = configuration.GetSection("JwtConfiguration").Get<JwtConfiguration>();

        Guard.Against.Null(jwtConfiguration);

        // In development, show detailed PII in logs
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            IdentityModelEventSource.ShowPII = true;
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtConfiguration.Issuer,
                ValidAudience = jwtConfiguration.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.Key)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
            };

            // Handle JWT from Authorization header (default behavior)
            o.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    // Don't throw exceptions for invalid tokens - just reject them silently
                    context.NoResult();
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}

public class JwtConfiguration
{
    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}
