using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Flights.UpdateFlight;

[Authorize]
public record UpdateFlightCommand(
    int Id,
    DateOnly Date,
    string Number,
    string IataCode,
    string IcaoCode,
    string Airline,
    FlightStatus Status,
    FlightSource FlightSource) : IRequest;

public class UpdateFlightCommandValidator : AbstractValidator<UpdateFlightCommand>
{
    public UpdateFlightCommandValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(20);
        RuleFor(x => x.IataCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.IcaoCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Airline).NotEmpty().MaximumLength(200);
    }
}

internal sealed class UpdateFlightCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateFlightCommand>
{
    public async Task Handle(UpdateFlightCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Flights
            .FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException(nameof(Flight), request.Id);

        entity.Date = request.Date;
        entity.Number = request.Number;
        entity.IataCode = request.IataCode;
        entity.IcaoCode = request.IcaoCode;
        entity.Airline = request.Airline;
        entity.Status = request.Status;
        entity.FlightSource = request.FlightSource;

        await context.SaveChangesAsync(cancellationToken);
    }
}
