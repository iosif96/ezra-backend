using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Touchpoints.CreateTouchpoint;

[Authorize]
public record CreateTouchpointCommand(int AirportId, int? TerminalId, int? GateId, string Label, float Latitude, float Longitude) : IRequest<int>;

public class CreateTouchpointCommandValidator : AbstractValidator<CreateTouchpointCommand>
{
    public CreateTouchpointCommandValidator()
    {
        RuleFor(x => x.AirportId).GreaterThan(0);
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
    }
}

internal sealed class CreateTouchpointCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateTouchpointCommand, int>
{
    public async Task<int> Handle(CreateTouchpointCommand request, CancellationToken cancellationToken)
    {
        var entity = new Touchpoint
        {
            AirportId = request.AirportId,
            TerminalId = request.TerminalId,
            GateId = request.GateId,
            Label = request.Label,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
        };

        context.Touchpoints.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
