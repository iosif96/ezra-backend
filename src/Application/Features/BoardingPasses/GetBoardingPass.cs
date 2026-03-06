using Application.Common.Security;
using Application.Domain.Entities;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.BoardingPasses.GetBoardingPass;

public record BoardingPassResponse(int Id, int FlightId, string Code, string? Seat, int IdentityId);

[Authorize]
public record GetBoardingPassQuery(int Id) : IRequest<BoardingPassResponse>;

internal sealed class GetBoardingPassQueryHandler(ApplicationDbContext context) : IRequestHandler<GetBoardingPassQuery, BoardingPassResponse>
{
    public async Task<BoardingPassResponse> Handle(GetBoardingPassQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.BoardingPasses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(BoardingPass), request.Id);

        return new BoardingPassResponse(entity.Id, entity.FlightId, entity.Code, entity.Seat, entity.IdentityId);
    }
}
