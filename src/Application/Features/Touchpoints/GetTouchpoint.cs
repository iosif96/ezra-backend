using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Touchpoints.GetTouchpoint;

public record TouchpointResponse(int Id, int AirportId, int? TerminalId, int? GateId, string Label, float Latitude, float Longitude);

[Authorize]
public record GetTouchpointQuery(int Id) : IRequest<TouchpointResponse>;

internal sealed class GetTouchpointQueryHandler(ApplicationDbContext context) : IRequestHandler<GetTouchpointQuery, TouchpointResponse>
{
    public async Task<TouchpointResponse> Handle(GetTouchpointQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Touchpoints
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Touchpoint), request.Id);

        return new TouchpointResponse(entity.Id, entity.AirportId, entity.TerminalId, entity.GateId, entity.Label, entity.Latitude, entity.Longitude);
    }
}
