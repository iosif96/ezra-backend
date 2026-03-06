using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.BoardingPasses.CreateBoardingPass;

[Authorize]
public record CreateBoardingPassCommand(int FlightId, string Code, string? Seat, int IdentityId) : IRequest<int>;

public class CreateBoardingPassCommandValidator : AbstractValidator<CreateBoardingPassCommand>
{
    public CreateBoardingPassCommandValidator()
    {
        RuleFor(x => x.FlightId).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IdentityId).GreaterThan(0);
    }
}

internal sealed class CreateBoardingPassCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateBoardingPassCommand, int>
{
    public async Task<int> Handle(CreateBoardingPassCommand request, CancellationToken cancellationToken)
    {
        var entity = new BoardingPass
        {
            FlightId = request.FlightId,
            Code = request.Code,
            Seat = request.Seat,
            IdentityId = request.IdentityId,
        };

        context.BoardingPasses.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
