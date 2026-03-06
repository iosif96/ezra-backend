using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Terminals.GetTerminal;

public record AirportDto(int Id, string Name, string IataCode, string IcaoCode);

public record TerminalResponse(int Id, AirportDto Airport, string Code, string PromptInformation);

[Authorize]
public record GetTerminalQuery(int Id) : IRequest<TerminalResponse>;

internal sealed class GetTerminalQueryHandler(ApplicationDbContext context) : IRequestHandler<GetTerminalQuery, TerminalResponse>
{
    public async Task<TerminalResponse> Handle(GetTerminalQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Terminals
            .Include(x => x.Airport)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Terminal), request.Id);

        var airportDto = new AirportDto(entity.Airport.Id, entity.Airport.Name, entity.Airport.IataCode, entity.Airport.IcaoCode);
        return new TerminalResponse(entity.Id, airportDto, entity.Code, entity.PromptInformation);
    }
}
