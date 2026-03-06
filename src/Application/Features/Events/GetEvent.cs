using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Events.GetEvent;

public record EventResponse(int Id, EventType Type, int? FlightId, DateTime? ScheduledOn, string Content);

[Authorize]
public record GetEventQuery(int Id) : IRequest<EventResponse>;

internal sealed class GetEventQueryHandler(ApplicationDbContext context) : IRequestHandler<GetEventQuery, EventResponse>
{
    public async Task<EventResponse> Handle(GetEventQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Application.Domain.Entities.Event), request.Id);

        return new EventResponse(entity.Id, entity.Type, entity.FlightId, entity.ScheduledOn, entity.Content);
    }
}
