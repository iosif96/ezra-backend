using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Requests.GetRequest;

public record FlightDto(int Id, DateOnly Date, string Number, string Airline, FlightStatus Status);

public record BoardingPassDto(int Id, string Code, string? Seat, FlightDto Flight);

public record IdentityDto(int Id, string? PassengerName, List<BoardingPassDto> BoardingPasses);

public record ConversationDto(int Id, ChannelType ChannelType, string ChannelId, IdentityDto? Identity);

public record StaffDto(int Id, string Name);

public record RequestResponse(int Id, ConversationDto Conversation, RequestType Type, string? Content, StaffDto? Staff, RequestStatus Status, DateTime Created);

[Authorize]
public record GetRequestQuery(int Id) : IRequest<RequestResponse>;

internal sealed class GetRequestQueryHandler(ApplicationDbContext context) : IRequestHandler<GetRequestQuery, RequestResponse>
{
    public async Task<RequestResponse> Handle(GetRequestQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Requests
            .AsNoTracking()
            .Include(x => x.Conversation).ThenInclude(x => x.Identity).ThenInclude(x => x!.BoardingPasses).ThenInclude(x => x.Flight)
            .Include(x => x.Staff)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Application.Domain.Entities.Request), request.Id);

        return new RequestResponse(
            entity.Id,
            new ConversationDto(
                entity.Conversation.Id,
                entity.Conversation.ChannelType,
                entity.Conversation.ChannelId,
                entity.Conversation.Identity != null ? new IdentityDto(
                    entity.Conversation.Identity.Id,
                    entity.Conversation.Identity.PassengerName,
                    entity.Conversation.Identity.BoardingPasses.Select(bp => new BoardingPassDto(
                        bp.Id,
                        bp.Code,
                        bp.Seat,
                        new FlightDto(bp.Flight.Id, bp.Flight.Date, bp.Flight.Number, bp.Flight.Airline, bp.Flight.Status)
                    )).ToList()) : null),
            entity.Type,
            entity.Content,
            entity.Staff != null ? new StaffDto(entity.Staff.Id, entity.Staff.Name) : null,
            entity.Status,
            entity.Created);
    }
}
