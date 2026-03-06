using Application.Common.Security;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Infrastructure.Persistence;

using FluentValidation;

namespace Application.Features.Messages.CreateMessage;

[Authorize]
public record CreateMessageCommand(
    int ConversationId,
    MessageType Type,
    MediaType MediaType,
    string? MediaUrl,
    string? Content,
    string? Model,
    int? InputTokens,
    int? OutputTokens,
    float? StressIndex) : IRequest<int>;

public class CreateMessageCommandValidator : AbstractValidator<CreateMessageCommand>
{
    public CreateMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).GreaterThan(0);
        RuleFor(x => x.StressIndex).InclusiveBetween(0f, 1f).When(x => x.StressIndex.HasValue);
    }
}

internal sealed class CreateMessageCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateMessageCommand, int>
{
    public async Task<int> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        var entity = new Message
        {
            ConversationId = request.ConversationId,
            Type = request.Type,
            MediaType = request.MediaType,
            MediaUrl = request.MediaUrl,
            Content = request.Content,
            Model = request.Model,
            InputTokens = request.InputTokens,
            OutputTokens = request.OutputTokens,
            StressIndex = request.StressIndex,
        };

        context.Messages.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
