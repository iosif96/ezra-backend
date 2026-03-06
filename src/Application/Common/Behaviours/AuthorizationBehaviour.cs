using System.Reflection;

using Application.Common.Interfaces;
using Application.Common.Security;

using Microsoft.AspNetCore.Http;

namespace Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IApiKeyService _apiKeyService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationBehaviour(
        ICurrentUserService currentUserService,
        IApiKeyService apiKeyService,
        IHttpContextAccessor httpContextAccessor)
    {
        _currentUserService = currentUserService;
        _apiKeyService = apiKeyService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();
        var allowApiKeyAttribute = request.GetType().GetCustomAttribute<AllowApiKeyAttribute>();

        if (authorizeAttributes.Any())
        {
            try
            {
                // If it's an API request, validate the API key
                if (_currentUserService.IsApiRequest)
                {
                    if (allowApiKeyAttribute == null)
                    {
                        throw new UnauthorizedAccessException("This endpoint cannot be accessed with an API key.");
                    }

                    var apiKey = _httpContextAccessor.HttpContext?.Request.Headers["X-API-Key"].ToString() ?? string.Empty;
                    var validApiKey = await _apiKeyService.ValidateApiKeyAsync(apiKey, cancellationToken);

                    if (validApiKey == null)
                    {
                        throw new UnauthorizedAccessException("Invalid API key or API key does not have access to this endpoint.");
                    }

                    // Continue with the request
                    return await next();
                }

                // Otherwise, check normal user authentication
                if (_currentUserService.UserId == null)
                {
                    throw new UnauthorizedAccessException("User is not authenticated.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Rethrow authorization-specific exceptions
            }
            catch (Exception ex)
            {
                // Wrap any other exceptions in an UnauthorizedAccessException
                throw new UnauthorizedAccessException($"Authorization failed: {ex.Message}");
            }
        }

        // User is authorized / authorization not required
        return await next();
    }
}