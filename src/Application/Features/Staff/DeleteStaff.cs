using Application.Common.Security;
using Application.Infrastructure.Persistence;

namespace Application.Features.Staff.DeleteStaff;

[Authorize]
public record DeleteStaffCommand(int Id) : IRequest;

internal sealed class DeleteStaffCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteStaffCommand>
{
    public async Task Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Staff
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Application.Domain.Entities.Staff), request.Id);

        context.Staff.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
