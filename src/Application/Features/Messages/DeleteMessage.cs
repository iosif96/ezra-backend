using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.Messages.DeleteMessage;

[Authorize]
public record DeleteMessageCommand(int Id) : IRequest;

internal sealed class DeleteMessageCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteMessageCommand>
{
    public async Task Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Messages
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Message), request.Id);

        context.Messages.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
