using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

namespace Application.Features.Requests.UpdateRequest;

[Authorize]
public record UpdateRequestCommand(int Id, int? StaffId, RequestStatus Status) : IRequest;

internal sealed class UpdateRequestCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateRequestCommand>
{
    public async Task Handle(UpdateRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Requests
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Application.Domain.Entities.Request), request.Id);

        entity.StaffId = request.StaffId;
        entity.Status = request.Status;

        await context.SaveChangesAsync(cancellationToken);
    }
}
