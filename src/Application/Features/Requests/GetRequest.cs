using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Requests.GetRequest;

public record IdentityDto(int Id, string? PassengerName);

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
            .Include(x => x.Conversation).ThenInclude(x => x.Identity)
            .Include(x => x.Staff)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Application.Domain.Entities.Request), request.Id);

        return new RequestResponse(
            entity.Id,
            new ConversationDto(
                entity.Conversation.Id,
                entity.Conversation.ChannelType,
                entity.Conversation.ChannelId,
                entity.Conversation.Identity != null ? new IdentityDto(entity.Conversation.Identity.Id, entity.Conversation.Identity.PassengerName) : null),
            entity.Type,
            entity.Content,
            entity.Staff != null ? new StaffDto(entity.Staff.Id, entity.Staff.Name) : null,
            entity.Status,
            entity.Created);
    }
}
