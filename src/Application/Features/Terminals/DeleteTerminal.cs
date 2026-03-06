using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.Terminals.DeleteTerminal;

[Authorize]
public record DeleteTerminalCommand(int Id) : IRequest;

internal sealed class DeleteTerminalCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteTerminalCommand>
{
    public async Task Handle(DeleteTerminalCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Terminals
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Terminal), request.Id);

        context.Terminals.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
