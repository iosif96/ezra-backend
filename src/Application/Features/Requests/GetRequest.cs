using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Requests.GetRequest;

public record RequestResponse(int Id, int ConversationId, RequestType Type, string? Content, int? StaffId, RequestStatus Status);

[Authorize]
public record GetRequestQuery(int Id) : IRequest<RequestResponse>;

internal sealed class GetRequestQueryHandler(ApplicationDbContext context) : IRequestHandler<GetRequestQuery, RequestResponse>
{
    public async Task<RequestResponse> Handle(GetRequestQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Requests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Application.Domain.Entities.Request), request.Id);

        return new RequestResponse(entity.Id, entity.ConversationId, entity.Type, entity.Content, entity.StaffId, entity.Status);
    }
}
