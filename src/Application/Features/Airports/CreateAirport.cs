using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Airports.CreateAirport;

[Authorize]
public record CreateAirportCommand(
    string Name,
    string IataCode,
    string IcaoCode,
    string City,
    string CountryCode,
    string DefaultLanguage,
    string PromptInformation) : IRequest<int>;

public class CreateAirportCommandValidator : AbstractValidator<CreateAirportCommand>
{
    public CreateAirportCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IataCode).NotEmpty().Length(3);
        RuleFor(x => x.IcaoCode).NotEmpty().Length(4);
        RuleFor(x => x.City).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CountryCode).NotEmpty().Length(2);
        RuleFor(x => x.DefaultLanguage).NotEmpty().MaximumLength(10);
    }
}

internal sealed class CreateAirportCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateAirportCommand, int>
{
    public async Task<int> Handle(CreateAirportCommand request, CancellationToken cancellationToken)
    {
        var entity = new Airport
        {
            Name = request.Name,
            IataCode = request.IataCode,
            IcaoCode = request.IcaoCode,
            City = request.City,
            CountryCode = request.CountryCode,
            DefaultLanguage = request.DefaultLanguage,
            PromptInformation = request.PromptInformation,
        };

        context.Airports.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
