using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.TouchpointScans.GetTouchpointScan;

public record TouchpointScanResponse(int Id, int TouchpointId, ChannelType ChannelType);

[Authorize]
public record GetTouchpointScanQuery(int Id) : IRequest<TouchpointScanResponse>;

internal sealed class GetTouchpointScanQueryHandler(ApplicationDbContext context) : IRequestHandler<GetTouchpointScanQuery, TouchpointScanResponse>
{
    public async Task<TouchpointScanResponse> Handle(GetTouchpointScanQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.TouchpointScans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(TouchpointScan), request.Id);

        return new TouchpointScanResponse(entity.Id, entity.TouchpointId, entity.ChannelType);
    }
}
