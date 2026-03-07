using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Requests.GetRequestsWithPagination;

public record FlightBriefDto(int Id, string Number, string IataCode, string Airline, FlightStatus Status);

public record BoardingPassBriefDto(string Code, string? Seat, FlightBriefDto Flight);

public record IdentityDto(int Id, string? PassengerName, IReadOnlyList<BoardingPassBriefDto> BoardingPasses);

public record ConversationDto(int Id, ChannelType ChannelType, string ChannelId, IdentityDto? Identity);

public record StaffDto(int Id, string Name, string PhoneNumber);

public record RequestBriefResponse(
    int Id,
    ConversationDto Conversation,
    RequestType Type,
    string? Content,
    StaffDto? Staff,
    RequestStatus Status,
    DateTime Created,
    DateTime? LastModified);

[Authorize]
public record GetRequestsWithPaginationQuery(int? ConversationId = null, RequestStatus? Status = null, RequestType? Type = null, int? StaffId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<RequestBriefResponse>>;

internal sealed class GetRequestsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetRequestsWithPaginationQuery, PaginatedList<RequestBriefResponse>>
{
    public Task<PaginatedList<RequestBriefResponse>> Handle(GetRequestsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Requests
            .Where(x => request.ConversationId == null || x.ConversationId == request.ConversationId)
            .Where(x => request.Status == null || x.Status == request.Status)
            .Where(x => request.Type == null || x.Type == request.Type)
            .Where(x => request.StaffId == null || x.StaffId == request.StaffId)
            .OrderByDescending(x => x.Created)
            .Select(x => new RequestBriefResponse(
                x.Id,
                new ConversationDto(
                    x.Conversation.Id,
                    x.Conversation.ChannelType,
                    x.Conversation.ChannelId,
                    x.Conversation.Identity != null ? new IdentityDto(
                        x.Conversation.Identity.Id,
                        x.Conversation.Identity.PassengerName,
                        x.Conversation.Identity.BoardingPasses.Select(bp => new BoardingPassBriefDto(
                            bp.Code,
                            bp.Seat,
                            new FlightBriefDto(bp.Flight.Id, bp.Flight.Number, bp.Flight.IataCode, bp.Flight.Airline, bp.Flight.Status)
                        )).ToList()
                    ) : null),
                x.Type,
                x.Content,
                x.Staff != null ? new StaffDto(x.Staff.Id, x.Staff.Name, x.Staff.PhoneNumber) : null,
                x.Status,
                x.Created,
                x.LastModified))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
