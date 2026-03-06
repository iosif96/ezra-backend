using Application.Common.Security;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Staff.CreateStaff;

[Authorize]
public record CreateStaffCommand(int AirportId, string Name, string PhoneNumber) : IRequest<int>;

public class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
{
    public CreateStaffCommandValidator()
    {
        RuleFor(x => x.AirportId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(50);
    }
}

internal sealed class CreateStaffCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateStaffCommand, int>
{
    public async Task<int> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var entity = new Application.Domain.Entities.Staff
        {
            AirportId = request.AirportId,
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
        };

        context.Staff.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
