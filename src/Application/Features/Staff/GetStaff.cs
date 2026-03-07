using Application.Common.Security;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Staff.GetStaff;

public record AirportDto(int Id, string Name, string IataCode);

public record StaffResponse(int Id, AirportDto Airport, string Name, string PhoneNumber);

[Authorize]
public record GetStaffQuery(int Id) : IRequest<StaffResponse>;

internal sealed class GetStaffQueryHandler(ApplicationDbContext context) : IRequestHandler<GetStaffQuery, StaffResponse>
{
    public async Task<StaffResponse> Handle(GetStaffQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Staff
            .AsNoTracking()
            .Include(x => x.Airport)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Application.Domain.Entities.Staff), request.Id);

        return new StaffResponse(
            entity.Id,
            new AirportDto(entity.Airport.Id, entity.Airport.Name, entity.Airport.IataCode),
            entity.Name,
            entity.PhoneNumber);
    }
}
