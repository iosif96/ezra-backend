using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Messages.GetMessage;

public record MessageResponse(
    int Id, int ConversationId, MessageType Type, MediaType MediaType,
    string? MediaUrl, string? Content, string? Model,
    int? InputTokens, int? OutputTokens, float? StressIndex);

[Authorize]
public record GetMessageQuery(int Id) : IRequest<MessageResponse>;

internal sealed class GetMessageQueryHandler(ApplicationDbContext context) : IRequestHandler<GetMessageQuery, MessageResponse>
{
    public async Task<MessageResponse> Handle(GetMessageQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Message), request.Id);

        return new MessageResponse(entity.Id, entity.ConversationId, entity.Type, entity.MediaType,
            entity.MediaUrl, entity.Content, entity.Model, entity.InputTokens, entity.OutputTokens, entity.StressIndex);
    }
}
