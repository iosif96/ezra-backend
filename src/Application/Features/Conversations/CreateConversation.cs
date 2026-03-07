using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Conversations.CreateConversation;

public record CreateConversationCommand(ChannelType ChannelType, string ChannelId, int? IdentityId) : IRequest<int>;

public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty().MaximumLength(200);
    }
}

internal sealed class CreateConversationCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateConversationCommand, int>
{
    public async Task<int> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        var entity = new Conversation
        {
            ChannelType = request.ChannelType,
            ChannelId = request.ChannelId,
            IdentityId = request.IdentityId,
            LastMessageOn = DateTime.UtcNow,
        };

        context.Conversations.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
