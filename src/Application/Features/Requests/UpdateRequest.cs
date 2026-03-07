using Application.Common.Security;
using Application.Domain.Enums;
using Application.Features.Conversations.SendSystemNotification;
using Application.Infrastructure.Persistence;

namespace Application.Features.Requests.UpdateRequest;

[Authorize]
public record UpdateRequestCommand(int Id, int? StaffId, RequestStatus Status) : IRequest;

internal sealed class UpdateRequestCommandHandler(ApplicationDbContext context, ISender sender) : IRequestHandler<UpdateRequestCommand>
{
    public async Task Handle(UpdateRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Requests
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Application.Domain.Entities.Request), request.Id);

        var previousStatus = entity.Status;

        entity.StaffId = request.StaffId;
        entity.Status = request.Status;

        await context.SaveChangesAsync(cancellationToken);

        if (previousStatus != RequestStatus.Confirmed && request.Status == RequestStatus.Confirmed)
        {
            await sender.Send(new SendSystemNotificationCommand(
                entity.ConversationId,
                $"[System: Airport staff has accepted and confirmed the {entity.Type} request. Please inform the passenger that help is on the way.]"), cancellationToken);
        }
    }
}
