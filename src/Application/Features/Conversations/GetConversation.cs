using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Conversations.GetConversation;

public record ConversationResponse(int Id, ChannelType ChannelType, string ChannelId, int? IdentityId, DateTime LastMessageOn);

[Authorize]
public record GetConversationQuery(int Id) : IRequest<ConversationResponse>;

internal sealed class GetConversationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetConversationQuery, ConversationResponse>
{
    public async Task<ConversationResponse> Handle(GetConversationQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Conversation), request.Id);

        return new ConversationResponse(entity.Id, entity.ChannelType, entity.ChannelId, entity.IdentityId, entity.LastMessageOn);
    }
}
