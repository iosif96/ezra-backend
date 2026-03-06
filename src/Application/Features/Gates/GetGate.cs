using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Gates.GetGate;

public record AirportDto(int Id, string Name, string IataCode, string IcaoCode);

public record TerminalDto(int Id, AirportDto Airport, string Code);

public record GateResponse(int Id, TerminalDto Terminal, string Code, string PromptInformation);

[Authorize]
public record GetGateQuery(int Id) : IRequest<GateResponse>;

internal sealed class GetGateQueryHandler(ApplicationDbContext context) : IRequestHandler<GetGateQuery, GateResponse>
{
    public async Task<GateResponse> Handle(GetGateQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Gates
            .Include(x => x.Terminal)
                .ThenInclude(x => x.Airport)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Gate), request.Id);

        var airportDto = new AirportDto(entity.Terminal.Airport.Id, entity.Terminal.Airport.Name, entity.Terminal.Airport.IataCode, entity.Terminal.Airport.IcaoCode);
        var terminalDto = new TerminalDto(entity.Terminal.Id, airportDto, entity.Terminal.Code);
        return new GateResponse(entity.Id, terminalDto, entity.Code, entity.PromptInformation);
    }
}
