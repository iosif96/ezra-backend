using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.TouchpointScans.CreateTouchpointScan;

[Authorize]
public record CreateTouchpointScanCommand(int TouchpointId, ChannelType ChannelType) : IRequest<int>;

public class CreateTouchpointScanCommandValidator : AbstractValidator<CreateTouchpointScanCommand>
{
    public CreateTouchpointScanCommandValidator()
    {
        RuleFor(x => x.TouchpointId).GreaterThan(0);
    }
}

internal sealed class CreateTouchpointScanCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateTouchpointScanCommand, int>
{
    public async Task<int> Handle(CreateTouchpointScanCommand request, CancellationToken cancellationToken)
    {
        var entity = new TouchpointScan
        {
            TouchpointId = request.TouchpointId,
            ChannelType = request.ChannelType,
        };

        context.TouchpointScans.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
