using Microsoft.Extensions.Configuration;

namespace Application.Common.Configuration;

public class TokenConfiguration
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenLifetimeInHours { get; set; } = 1;
    public int RefreshTokenLifetimeInDays { get; set; } = 14;
    public int OtpLifetimeInMinutes { get; set; } = 5;

    public TimeSpan AccessTokenLifetime => TimeSpan.FromHours(AccessTokenLifetimeInHours);
    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(RefreshTokenLifetimeInDays);
    public TimeSpan OtpLifetime => TimeSpan.FromMinutes(OtpLifetimeInMinutes);

    public static TokenConfiguration FromConfiguration(IConfiguration configuration)
    {
        var config = new TokenConfiguration();
        configuration.GetSection("JwtConfiguration").Bind(config);
        return config;
    }
}