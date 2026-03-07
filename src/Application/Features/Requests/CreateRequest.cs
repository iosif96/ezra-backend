using Application.Common.Interfaces;
using Application.Common.Security;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.Requests.CreateRequest;

[Authorize]
public record CreateRequestCommand(int ConversationId, RequestType Type, string? Content) : IRequest<int>;

public class CreateRequestCommandValidator : AbstractValidator<CreateRequestCommand>
{
    public CreateRequestCommandValidator()
    {
        RuleFor(x => x.ConversationId).GreaterThan(0);
    }
}

internal sealed class CreateRequestCommandHandler(ApplicationDbContext context, IOverviewNotifier overviewNotifier) : IRequestHandler<CreateRequestCommand, int>
{
    public async Task<int> Handle(CreateRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = new Application.Domain.Entities.Request
        {
            ConversationId = request.ConversationId,
            Type = request.Type,
            Content = request.Content,
            Status = RequestStatus.Requested,
        };

        context.Requests.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        var channelType = await context.Conversations
            .Where(c => c.Id == request.ConversationId)
            .Select(c => c.ChannelType)
            .FirstAsync(cancellationToken);

        await overviewNotifier.NotifyRequestCreated(new RequestCreatedNotification(
            entity.Id, entity.Type, entity.Content, entity.Status, channelType, entity.Created), cancellationToken);

        return entity.Id;
    }
}
