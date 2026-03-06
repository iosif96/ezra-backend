using System.Reflection;

using Application.Common.Interfaces;
using Application.Common.Interfaces.BlobStorage;
using Application.Common.Models.BlobStorage;
using Application.Infrastructure.Files;
using Application.Infrastructure.Persistence;
using Application.Infrastructure.Persistence.Interceptors;
using Application.Infrastructure.Services;
using Application.Infrastructure.Services.BlobStorage;

using Ardalis.GuardClauses;

using FluentValidation;

using Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, SoftDeleteInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseSqlServer(connectionString);
        });

        services.Configure<AzureStorageOptions>(configuration.GetSection(AzureStorageOptions.AzureStorage));

        services.AddScoped<ApplicationDbContextInitialiser>();

        services.AddSingleton<IConcreteStorageClient, ConcreteStorageClient>();
        services.AddScoped<IDomainEventService, DomainEventService>();

        services.AddTransient<ICsvFileBuilder, CsvFileBuilder>();
        services.AddScoped<IMailService, MailService>();
        services.AddScoped<IGeminiService, GeminiService>();
        services.AddScoped<ICryptographyService, CryptographyService>();

        services.AddSingleton<IThumbnailService, ThumbnailService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();

        services.AddSingleton<ITokenService, TokenService>();

        services.AddSingleton<IDateTime, DateTimeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
