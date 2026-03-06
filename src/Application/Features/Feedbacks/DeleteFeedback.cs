using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.Feedbacks.DeleteFeedback;

[Authorize]
public record DeleteFeedbackCommand(int Id) : IRequest;

internal sealed class DeleteFeedbackCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteFeedbackCommand>
{
    public async Task Handle(DeleteFeedbackCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Feedbacks
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Feedback), request.Id);

        context.Feedbacks.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
