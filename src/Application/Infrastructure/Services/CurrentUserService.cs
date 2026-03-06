using System.Security.Claims;

using Application.Common.Interfaces;

using Microsoft.AspNetCore.Http;

namespace Application.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        ITokenService tokenService,
        IHttpContextAccessor httpContextAccessor)
    {
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId => GetCurrentUserId();
    public string DeviceInfo => GetDeviceInfo();
    public bool IsApiRequest => GetIsApiRequest();

    public int? GetCurrentUserId()
    {
        // First check if it's an API request
        if (IsApiRequest)
        {
            // We'll use a different mechanism to validate API keys
            // This will be handled by ApiKeyAuthorizationBehavior
            // to avoid circular dependencies
            return null;
        }

        // Fall back to JWT authentication from Authorization header
        try
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            var jwtToken = authHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(jwtToken))
            {
                return null;
            }

            var token = _tokenService.ValidateToken(jwtToken);

            // Check if validation was successful (token has claims)
            if (token.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var userIdClaim = token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return null;
            }

            if (int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            // Suppress any exceptions and return null for unauthenticated users
            return null;
        }
    }

    private string GetDeviceInfo()
    {
        // For mobile apps get the device info from the CUSTOM request headers
        var deviceInfo = _httpContextAccessor.HttpContext?.Request.Headers["DeviceInfo"].ToString() ?? "Unknown";
        if (!string.IsNullOrEmpty(deviceInfo))
        {
            return deviceInfo;
        }

        // For web browsers get the device info from the user agent header
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? "Unknown";
        return userAgent;
    }

    private bool GetIsApiRequest()
    {
        var apiKey = _httpContextAccessor.HttpContext?.Request.Headers["X-API-Key"].ToString();
        return !string.IsNullOrEmpty(apiKey);
    }
}