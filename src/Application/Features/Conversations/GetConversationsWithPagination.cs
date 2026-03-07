using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Conversations.GetConversationsWithPagination;

public record IdentityBriefDto(int Id, string? PassengerName);

public record FlightBriefDto(int Id, string Number, string IataCode, string Airline, FlightStatus Status);

public record BoardingPassBriefDto(string Code, string? Seat, FlightBriefDto Flight);

public record IdentityWithFlightsDto(int Id, string? PassengerName, IReadOnlyList<BoardingPassBriefDto> BoardingPasses);

public record ConversationBriefResponse(
    int Id,
    ChannelType ChannelType,
    string ChannelId,
    IdentityWithFlightsDto? Identity,
    int MessageCount,
    string? LastMessagePreview,
    DateTime LastMessageOn,
    DateTime Created);

[Authorize]
public record GetConversationsWithPaginationQuery(ChannelType? ChannelType = null, int? IdentityId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<ConversationBriefResponse>>;

internal sealed class GetConversationsWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetConversationsWithPaginationQuery, PaginatedList<ConversationBriefResponse>>
{
    public Task<PaginatedList<ConversationBriefResponse>> Handle(GetConversationsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Conversations
            .Where(x => request.ChannelType == null || x.ChannelType == request.ChannelType)
            .Where(x => request.IdentityId == null || x.IdentityId == request.IdentityId)
            .OrderByDescending(x => x.LastMessageOn)
            .Select(x => new ConversationBriefResponse(
                x.Id,
                x.ChannelType,
                x.ChannelId,
                x.Identity != null ? new IdentityWithFlightsDto(
                    x.Identity.Id,
                    x.Identity.PassengerName,
                    x.Identity.BoardingPasses.Select(bp => new BoardingPassBriefDto(
                        bp.Code,
                        bp.Seat,
                        new FlightBriefDto(bp.Flight.Id, bp.Flight.Number, bp.Flight.IataCode, bp.Flight.Airline, bp.Flight.Status)
                    )).ToList()
                ) : null,
                x.Messages.Count,
                x.Messages.OrderByDescending(m => m.Id).Select(m => m.Content).FirstOrDefault(),
                x.LastMessageOn,
                x.Created))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
