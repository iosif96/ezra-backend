using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

namespace Application.Features.TouchpointScans.UpdateTouchpointScan;

[Authorize]
public record UpdateTouchpointScanCommand(int Id, ChannelType ChannelType) : IRequest;

internal sealed class UpdateTouchpointScanCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateTouchpointScanCommand>
{
    public async Task Handle(UpdateTouchpointScanCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.TouchpointScans
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(TouchpointScan), request.Id);

        entity.ChannelType = request.ChannelType;

        await context.SaveChangesAsync(cancellationToken);
    }
}
