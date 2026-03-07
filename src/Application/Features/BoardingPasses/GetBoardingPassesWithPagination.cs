using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Security;
using Application.Infrastructure.Persistence;

namespace Application.Features.BoardingPasses.GetBoardingPassesWithPagination;

public record BoardingPassIdentityDto(int Id, string? PassengerName);

public record BoardingPassBriefResponse(int Id, int FlightId, string Code, string? Seat, BoardingPassIdentityDto Identity);

[Authorize]
public record GetBoardingPassesWithPaginationQuery(int? FlightId = null, int? IdentityId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<BoardingPassBriefResponse>>;

internal sealed class GetBoardingPassesWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetBoardingPassesWithPaginationQuery, PaginatedList<BoardingPassBriefResponse>>
{
    public Task<PaginatedList<BoardingPassBriefResponse>> Handle(GetBoardingPassesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.BoardingPasses
            .Where(x => request.FlightId == null || x.FlightId == request.FlightId)
            .Where(x => request.IdentityId == null || x.IdentityId == request.IdentityId)
            .OrderBy(x => x.Id)
            .Select(x => new BoardingPassBriefResponse(x.Id, x.FlightId, x.Code, x.Seat, new BoardingPassIdentityDto(x.IdentityId, x.Identity.PassengerName)))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
