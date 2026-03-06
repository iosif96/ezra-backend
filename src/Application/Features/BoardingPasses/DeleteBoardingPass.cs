using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.BoardingPasses.DeleteBoardingPass;

[Authorize]
public record DeleteBoardingPassCommand(int Id) : IRequest;

internal sealed class DeleteBoardingPassCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteBoardingPassCommand>
{
    public async Task Handle(DeleteBoardingPassCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.BoardingPasses
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(BoardingPass), request.Id);

        context.BoardingPasses.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
