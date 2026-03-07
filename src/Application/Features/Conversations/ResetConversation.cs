using Application.Common.Security;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Conversations.ResetConversation;

[Authorize]
public record ResetConversationCommand(int Id) : IRequest;

internal sealed class ResetConversationCommandHandler(ApplicationDbContext context) : IRequestHandler<ResetConversationCommand>
{
    public async Task Handle(ResetConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await context.Conversations
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Conversation), request.Id);

        var messages = await context.Messages
            .Where(m => m.ConversationId == request.Id)
            .ToListAsync(cancellationToken);

        context.Messages.RemoveRange(messages);
        conversation.LastMessageOn = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}
