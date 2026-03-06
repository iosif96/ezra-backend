using Application.Common.Interfaces;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.GetUserProfile;

public record GetUserProfileResponse(int Id, string? UserName, string Email, Role Role);

public record GetUserProfileCommand() : IRequest<GetUserProfileResponse>;

internal sealed class GetUserProfileCommandHandler(ApplicationDbContext context, ICurrentUserService currentUserService) : IRequestHandler<GetUserProfileCommand, GetUserProfileResponse>
{
    public async Task<GetUserProfileResponse> Handle(GetUserProfileCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == currentUserService.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        return new GetUserProfileResponse(
            currentUser.Id,
            currentUser.UserName,
            currentUser.Email,
            currentUser.Role);
    }
}
