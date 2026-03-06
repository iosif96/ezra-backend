using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Touchpoints.UpdateTouchpoint;

[Authorize]
public record UpdateTouchpointCommand(int Id, int? TerminalId, int? GateId, string Label, float Latitude, float Longitude) : IRequest;

public class UpdateTouchpointCommandValidator : AbstractValidator<UpdateTouchpointCommand>
{
    public UpdateTouchpointCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
    }
}

internal sealed class UpdateTouchpointCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateTouchpointCommand>
{
    public async Task Handle(UpdateTouchpointCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Touchpoints
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Touchpoint), request.Id);

        entity.TerminalId = request.TerminalId;
        entity.GateId = request.GateId;
        entity.Label = request.Label;
        entity.Latitude = request.Latitude;
        entity.Longitude = request.Longitude;

        await context.SaveChangesAsync(cancellationToken);
    }
}
