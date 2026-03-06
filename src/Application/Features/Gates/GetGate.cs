using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Gates.GetGate;

public record GateResponse(int Id, int TerminalId, string Code, string PromptInformation);

[Authorize]
public record GetGateQuery(int Id) : IRequest<GateResponse>;

internal sealed class GetGateQueryHandler(ApplicationDbContext context) : IRequestHandler<GetGateQuery, GateResponse>
{
    public async Task<GateResponse> Handle(GetGateQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Gates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Gate), request.Id);

        return new GateResponse(entity.Id, entity.TerminalId, entity.Code, entity.PromptInformation);
    }
}
