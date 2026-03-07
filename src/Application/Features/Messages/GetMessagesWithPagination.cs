using Application.Common.Mappings;
using Application.Common.Models;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

namespace Application.Features.Messages.GetMessagesWithPagination;

public record MessageBriefResponse(int Id, int ConversationId, MessageType Type, MediaType MediaType, string? Content);

public record GetMessagesWithPaginationQuery(int? ConversationId = null, MessageType? Type = null, int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedList<MessageBriefResponse>>;

internal sealed class GetMessagesWithPaginationQueryHandler(ApplicationDbContext context) : IRequestHandler<GetMessagesWithPaginationQuery, PaginatedList<MessageBriefResponse>>
{
    public Task<PaginatedList<MessageBriefResponse>> Handle(GetMessagesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return context.Messages
            .Where(x => request.ConversationId == null || x.ConversationId == request.ConversationId)
            .Where(x => request.Type == null || x.Type == request.Type)
            .OrderBy(x => x.Id)
            .Select(x => new MessageBriefResponse(x.Id, x.ConversationId, x.Type, x.MediaType, x.Content))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
