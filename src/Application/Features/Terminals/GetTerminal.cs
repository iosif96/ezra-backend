using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Terminals.GetTerminal;

public record TerminalResponse(int Id, int AirportId, string Code, string PromptInformation);

[Authorize]
public record GetTerminalQuery(int Id) : IRequest<TerminalResponse>;

internal sealed class GetTerminalQueryHandler(ApplicationDbContext context) : IRequestHandler<GetTerminalQuery, TerminalResponse>
{
    public async Task<TerminalResponse> Handle(GetTerminalQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Terminals
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Terminal), request.Id);

        return new TerminalResponse(entity.Id, entity.AirportId, entity.Code, entity.PromptInformation);
    }
}
