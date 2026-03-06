using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Users.ChangeUserRole;

public record ChangeUserRoleResponse(int Id, string? UserName, string Email, Role Role);

[Authorize(Roles = "Admin")]
public record ChangeUserRoleCommand(int UserId, Role Role) : IRequest<ChangeUserRoleResponse>;

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role must be a valid role.");
    }
}

internal sealed class ChangeUserRoleCommandHandler(ApplicationDbContext context) : IRequestHandler<ChangeUserRoleCommand, ChangeUserRoleResponse>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ChangeUserRoleResponse> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync([request.UserId], cancellationToken)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found.");

        user.Role = request.Role;
        await _context.SaveChangesAsync(cancellationToken);

        return new ChangeUserRoleResponse(user.Id, user.UserName, user.Email, user.Role);
    }
}
