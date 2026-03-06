using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Merchants.UpdateMerchant;

[Authorize]
public record UpdateMerchantCommand(int Id, string Name, bool IsAirside, float Latitude, float Longitude, string PromptInformation) : IRequest;

public class UpdateMerchantCommandValidator : AbstractValidator<UpdateMerchantCommand>
{
    public UpdateMerchantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

internal sealed class UpdateMerchantCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateMerchantCommand>
{
    public async Task Handle(UpdateMerchantCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Merchants
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Merchant), request.Id);

        entity.Name = request.Name;
        entity.IsAirside = request.IsAirside;
        entity.Latitude = request.Latitude;
        entity.Longitude = request.Longitude;
        entity.PromptInformation = request.PromptInformation;

        await context.SaveChangesAsync(cancellationToken);
    }
}
