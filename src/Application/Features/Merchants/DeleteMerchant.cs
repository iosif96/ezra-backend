using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

namespace Application.Features.Merchants.DeleteMerchant;

[Authorize]
public record DeleteMerchantCommand(int Id) : IRequest;

internal sealed class DeleteMerchantCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteMerchantCommand>
{
    public async Task Handle(DeleteMerchantCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Merchants
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Merchant), request.Id);

        context.Merchants.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
