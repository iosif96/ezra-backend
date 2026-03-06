using Application.Common.Interfaces;
using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.ApiKeys.CreateApiKey;

[Authorize]
public record CreateApiKeyCommand(string Name, DateTime? ExpiresAt) : IRequest<ApiKeyResponse>;

public record ApiKeyResponse(string Key, string Name, DateTime CreatedAt, DateTime? ExpiresAt);

public class CreateApiKeyCommandHandler(ApplicationDbContext context, ICurrentUserService currentUserService) : IRequestHandler<CreateApiKeyCommand, ApiKeyResponse>
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    public async Task<ApiKeyResponse> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var apiKey = new ApiKey
        {
            UserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException(),
            Name = request.Name,
            Key = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            IsActive = true,
        };

        try
        {
            await _context.ApiKeys.AddAsync(apiKey, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to create API key.", ex);
        }

        return new ApiKeyResponse(apiKey.Key, apiKey.Name, apiKey.CreatedAt, apiKey.ExpiresAt);
    }
}