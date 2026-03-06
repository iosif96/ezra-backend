using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Airports.UpdateAirport;

[Authorize]
public record UpdateAirportCommand(
    int Id,
    string Name,
    string IataCode,
    string IcaoCode,
    string City,
    string CountryCode,
    string DefaultLanguage,
    string PromptInformation) : IRequest;

public class UpdateAirportCommandValidator : AbstractValidator<UpdateAirportCommand>
{
    public UpdateAirportCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IataCode).NotEmpty().Length(3);
        RuleFor(x => x.IcaoCode).NotEmpty().Length(4);
        RuleFor(x => x.City).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CountryCode).NotEmpty().Length(2);
        RuleFor(x => x.DefaultLanguage).NotEmpty().MaximumLength(10);
    }
}

internal sealed class UpdateAirportCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateAirportCommand>
{
    public async Task Handle(UpdateAirportCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Airports
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Airport), request.Id);

        entity.Name = request.Name;
        entity.IataCode = request.IataCode;
        entity.IcaoCode = request.IcaoCode;
        entity.City = request.City;
        entity.CountryCode = request.CountryCode;
        entity.DefaultLanguage = request.DefaultLanguage;
        entity.PromptInformation = request.PromptInformation;

        await context.SaveChangesAsync(cancellationToken);
    }
}
