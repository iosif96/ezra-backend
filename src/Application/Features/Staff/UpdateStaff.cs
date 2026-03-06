using Application.Common.Security;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Staff.UpdateStaff;

[Authorize]
public record UpdateStaffCommand(int Id, string Name, string PhoneNumber) : IRequest;

public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(50);
    }
}

internal sealed class UpdateStaffCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateStaffCommand>
{
    public async Task Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Staff
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Application.Domain.Entities.Staff), request.Id);

        entity.Name = request.Name;
        entity.PhoneNumber = request.PhoneNumber;

        await context.SaveChangesAsync(cancellationToken);
    }
}
