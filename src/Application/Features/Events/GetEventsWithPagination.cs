using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

namespace Application.Features.Events.GetEventsWithPagination;

public record EventFlightDto(int Id, string Number, string IataCode, string Airline);

public record EventBriefResponse(int Id, EventType Type, EventFlightDto? Flight, DateTime? ScheduledOn, string Content, bool HasMessages);

[Authorize]
public record GetEventsWithPaginationQuery(EventType? Type = null, int? FlightId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<EventBriefResponse>>;

internal sealed class GetEventsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetEventsWithPaginationQuery, PaginatedList<EventBriefResponse>>
{
    public Task<PaginatedList<EventBriefResponse>> Handle(GetEventsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Events
            .Where(x => request.Type == null || x.Type == request.Type)
            .Where(x => request.FlightId == null || x.FlightId == request.FlightId)
            .OrderByDescending(x => x.Id)
            .Select(x => new EventBriefResponse(
                x.Id,
                x.Type,
                x.Flight != null ? new EventFlightDto(x.Flight.Id, x.Flight.Number, x.Flight.IataCode, x.Flight.Airline) : null,
                x.ScheduledOn,
                x.Content,
                x.Messages.Count > 0))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
