using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Flights.CreateFlight;

[Authorize]
public record CreateFlightCommand(
    DateOnly Date,
    string Number,
    string IataCode,
    string IcaoCode,
    string Airline,
    FlightStatus Status,
    FlightSource FlightSource) : IRequest<int>;

public class CreateFlightCommandValidator : AbstractValidator<CreateFlightCommand>
{
    public CreateFlightCommandValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(20);
        RuleFor(x => x.IataCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.IcaoCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Airline).NotEmpty().MaximumLength(200);
    }
}

internal sealed class CreateFlightCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateFlightCommand, int>
{
    public async Task<int> Handle(CreateFlightCommand request, CancellationToken cancellationToken)
    {
        var entity = new Flight
        {
            Date = request.Date,
            Number = request.Number,
            IataCode = request.IataCode,
            IcaoCode = request.IcaoCode,
            Airline = request.Airline,
            Status = request.Status,
            FlightSource = request.FlightSource,
        };

        context.Flights.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
