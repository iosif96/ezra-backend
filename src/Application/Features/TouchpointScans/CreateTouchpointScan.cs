using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.TouchpointScans.CreateTouchpointScan;

public record CreateTouchpointScanResponse(
    int ScanId,
    TouchpointInfo Touchpoint);

public record TouchpointInfo(
    int Id,
    string Label,
    string AirportCode);

public record CreateTouchpointScanCommand(int TouchpointId, ChannelType ChannelType) : IRequest<CreateTouchpointScanResponse>;

public class CreateTouchpointScanCommandValidator : AbstractValidator<CreateTouchpointScanCommand>
{
    public CreateTouchpointScanCommandValidator()
    {
        RuleFor(x => x.TouchpointId).GreaterThan(0);
    }
}

internal sealed class CreateTouchpointScanCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateTouchpointScanCommand, CreateTouchpointScanResponse>
{
    public async Task<CreateTouchpointScanResponse> Handle(CreateTouchpointScanCommand request, CancellationToken cancellationToken)
    {
        var touchpoint = await context.Touchpoints
            .Include(t => t.Airport)
            .FirstOrDefaultAsync(t => t.Id == request.TouchpointId, cancellationToken)
            ?? throw new NotFoundException(nameof(Touchpoint), request.TouchpointId);

        var entity = new TouchpointScan
        {
            TouchpointId = request.TouchpointId,
            ChannelType = request.ChannelType,
        };

        context.TouchpointScans.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateTouchpointScanResponse(
            entity.Id,
            new TouchpointInfo(
                touchpoint.Id,
                touchpoint.Label,
                touchpoint.Airport.IataCode));
    }
}
