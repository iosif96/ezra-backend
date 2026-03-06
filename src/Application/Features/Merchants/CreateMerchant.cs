using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Merchants.CreateMerchant;

[Authorize]
public record CreateMerchantCommand(int TerminalId, string Name, bool IsAirside, float Latitude, float Longitude, string PromptInformation) : IRequest<int>;

public class CreateMerchantCommandValidator : AbstractValidator<CreateMerchantCommand>
{
    public CreateMerchantCommandValidator()
    {
        RuleFor(x => x.TerminalId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

internal sealed class CreateMerchantCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateMerchantCommand, int>
{
    public async Task<int> Handle(CreateMerchantCommand request, CancellationToken cancellationToken)
    {
        var entity = new Merchant
        {
            TerminalId = request.TerminalId,
            Name = request.Name,
            IsAirside = request.IsAirside,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            PromptInformation = request.PromptInformation,
        };

        context.Merchants.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
