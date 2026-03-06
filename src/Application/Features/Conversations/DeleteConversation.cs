using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.Conversations.DeleteConversation;

[Authorize]
public record DeleteConversationCommand(int Id) : IRequest;

internal sealed class DeleteConversationCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteConversationCommand>
{
    public async Task Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Conversations
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Conversation), request.Id);

        context.Conversations.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
