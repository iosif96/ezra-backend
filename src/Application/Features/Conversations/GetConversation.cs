using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Conversations.GetConversation;

public record FlightBriefDto(int Id, string Number, string IataCode, string Airline, FlightStatus Status);

public record BoardingPassBriefDto(string Code, string? Seat, FlightBriefDto Flight);

public record IdentityWithFlightsDto(int Id, string? PassengerName, IReadOnlyList<BoardingPassBriefDto> BoardingPasses);

public record ConversationResponse(int Id, ChannelType ChannelType, string ChannelId, IdentityWithFlightsDto? Identity, DateTime LastMessageOn, DateTime Created);

[Authorize]
public record GetConversationQuery(int Id) : IRequest<ConversationResponse>;

internal sealed class GetConversationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetConversationQuery, ConversationResponse>
{
    public async Task<ConversationResponse> Handle(GetConversationQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Conversations
            .AsNoTracking()
            .Include(x => x.Identity)
                .ThenInclude(i => i!.BoardingPasses)
                    .ThenInclude(bp => bp.Flight)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Conversation), request.Id);

        return new ConversationResponse(
            entity.Id,
            entity.ChannelType,
            entity.ChannelId,
            entity.Identity != null ? new IdentityWithFlightsDto(
                entity.Identity.Id,
                entity.Identity.PassengerName,
                entity.Identity.BoardingPasses.Select(bp => new BoardingPassBriefDto(
                    bp.Code,
                    bp.Seat,
                    new FlightBriefDto(bp.Flight.Id, bp.Flight.Number, bp.Flight.IataCode, bp.Flight.Airline, bp.Flight.Status)
                )).ToList()
            ) : null,
            entity.LastMessageOn,
            entity.Created);
    }
}
