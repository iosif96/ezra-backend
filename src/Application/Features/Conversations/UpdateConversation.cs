using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.Conversations.UpdateConversation;

[Authorize]
public record UpdateConversationCommand(int Id, int? IdentityId, DateTime LastMessageOn) : IRequest;

internal sealed class UpdateConversationCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateConversationCommand>
{
    public async Task Handle(UpdateConversationCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Conversations
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Conversation), request.Id);

        entity.IdentityId = request.IdentityId;
        entity.LastMessageOn = request.LastMessageOn;

        await context.SaveChangesAsync(cancellationToken);
    }
}
