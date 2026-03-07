using Application.Common.Interfaces;
using Application.Common.Models.Chat;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Features.Conversations.Prompts;
using Application.Features.Conversations.Tools;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Features.Conversations.SendSystemNotification;

/// <summary>
/// Inserts a system message into a conversation, prompts the LLM for a response,
/// saves the assistant reply, and sends it to the user via their channel.
/// Use this from anywhere that needs the AI to notify the user about something.
/// </summary>
public record SendSystemNotificationCommand(int ConversationId, string Text) : IRequest;

internal sealed class SendSystemNotificationHandler(
    ApplicationDbContext context,
    IChatCompletionService completionService,
    IMessagingChannel messagingChannel,
    IEnumerable<IChatTool> tools,
    PromptBuilder promptBuilder,
    IOverviewNotifier overviewNotifier,
    IConfiguration configuration,
    ILogger<SendSystemNotificationHandler> logger) : IRequestHandler<SendSystemNotificationCommand>
{
    private const int MaxToolRounds = 5;

    public async Task Handle(SendSystemNotificationCommand command, CancellationToken cancellationToken)
    {
        var conversation = await context.Conversations
            .FirstOrDefaultAsync(c => c.Id == command.ConversationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Conversation), command.ConversationId);

        // Save the system message
        conversation.LastMessageOn = DateTime.UtcNow;

        context.Messages.Add(new Message
        {
            ConversationId = conversation.Id,
            Type = MessageType.System,
            MediaType = MediaType.Text,
            Content = command.Text,
        });

        await context.SaveChangesAsync(cancellationToken);

        // Load history and get AI response
        var history = await LoadHistoryAsync(conversation.Id, cancellationToken);
        var (responseText, inputTokens, outputTokens) = await GetAssistantResponseAsync(conversation, history, cancellationToken);

        // Save the assistant message
        var model = configuration["AnthropicConfiguration:Model"] ?? "claude-sonnet-4-5-20250514";

        context.Messages.Add(new Message
        {
            ConversationId = conversation.Id,
            Type = MessageType.Assistant,
            MediaType = MediaType.Text,
            Content = responseText,
            Model = model,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
        });

        await context.SaveChangesAsync(cancellationToken);

        // Broadcast live update
        var now = DateTime.UtcNow;
        var oneHourAgo = now.AddHours(-1);
        var todayStart = now.Date;

        await overviewNotifier.NotifyConversationUpdate(new ConversationUpdate(
            await context.Messages.CountAsync(m => m.Created >= oneHourAgo, cancellationToken),
            await context.Conversations.CountAsync(c => c.LastMessageOn >= oneHourAgo, cancellationToken),
            await context.Conversations.CountAsync(c => c.LastMessageOn >= todayStart, cancellationToken)), cancellationToken);

        // Send reply to the user
        if (conversation.ChannelType != ChannelType.Web)
        {
            var parts = MessageSplitter.SplitIntoMessages(responseText);

            foreach (var part in parts)
            {
                await messagingChannel.SendTextMessageAsync(conversation.ChannelId, part, cancellationToken);
            }
        }
    }

    private async Task<List<ChatMessageDto>> LoadHistoryAsync(int conversationId, CancellationToken cancellationToken)
    {
        var messages = await context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.Created)
            .ToListAsync(cancellationToken);

        var history = new List<ChatMessageDto>();

        foreach (var msg in messages)
        {
            if (string.IsNullOrEmpty(msg.Content))
            {
                continue;
            }

            var role = msg.Type == MessageType.Assistant
                ? ChatMessageRole.Assistant
                : ChatMessageRole.User;

            var text = role == ChatMessageRole.User
                ? $"[{msg.Created:yyyy-MM-dd HH:mm} UTC] {msg.Content}"
                : msg.Content;

            history.Add(ChatMessageDto.FromText(role, text));
        }

        return history;
    }

    private async Task<(string Text, int InputTokens, int OutputTokens)> GetAssistantResponseAsync(
        Conversation conversation, List<ChatMessageDto> history, CancellationToken cancellationToken)
    {
        var model = configuration["AnthropicConfiguration:Model"] ?? "claude-sonnet-4-5-20250514";
        var toolContext = new ToolContext { ConversationId = conversation.Id, IdentityId = conversation.IdentityId };
        var availableTools = tools.Where(t => t.IsAvailable(toolContext)).ToList();
        var toolDefinitions = availableTools.Select(t => t.ToDefinition()).ToList();

        var request = new ChatCompletionRequest
        {
            Model = model,
            SystemPrompt = promptBuilder.Build(),
            Messages = history,
            Tools = toolDefinitions.Count > 0 ? toolDefinitions : null,
            MaxTokens = 4096,
        };

        var totalInputTokens = 0;
        var totalOutputTokens = 0;

        var result = await completionService.CompleteAsync(request, cancellationToken);
        totalInputTokens += result.InputTokens;
        totalOutputTokens += result.OutputTokens;

        var round = 0;
        while (result.RequiresToolUse && round < MaxToolRounds)
        {
            round++;

            var toolCalls = result.GetToolCalls();

            var toolCallSummary = string.Join("\n", toolCalls.Select(t => $"[Tool: {t.Name}] {t.Input}"));
            context.Messages.Add(new Message
            {
                ConversationId = conversation.Id,
                Type = MessageType.Assistant,
                MediaType = MediaType.Text,
                Content = toolCallSummary,
            });

            history.Add(new ChatMessageDto
            {
                Role = ChatMessageRole.Assistant,
                Content = result.Content,
            });

            var toolResults = await ExecuteToolCallsAsync(toolCalls, availableTools, toolContext, cancellationToken);

            // Send tool labels to the user
            foreach (var toolCall in toolCalls)
            {
                var tool = availableTools.FirstOrDefault(t => t.Name == toolCall.Name);
                if (tool is not null)
                {
                    var isError = toolResults.OfType<ToolResultContent>()
                        .FirstOrDefault(r => r.ToolUseId == toolCall.Id)?.IsError ?? false;
                    var label = isError ? $"> {tool.UserLabel} failed" : $"> {tool.UserLabel}";
                    await messagingChannel.SendTextMessageAsync(conversation.ChannelId, label, cancellationToken);
                }
            }

            var resultParts = toolCalls.Zip(
                toolResults.OfType<ToolResultContent>(),
                (call, res) => $"[Tool result: {call.Name}] {res.Content}");
            context.Messages.Add(new Message
            {
                ConversationId = conversation.Id,
                Type = MessageType.System,
                MediaType = MediaType.Text,
                Content = string.Join("\n", resultParts),
            });

            history.Add(new ChatMessageDto
            {
                Role = ChatMessageRole.User,
                Content = toolResults,
            });

            await context.SaveChangesAsync(cancellationToken);

            result = await completionService.CompleteAsync(request, cancellationToken);
            totalInputTokens += result.InputTokens;
            totalOutputTokens += result.OutputTokens;
        }

        return (result.GetText() ?? "", totalInputTokens, totalOutputTokens);
    }

    private async Task<List<ContentBlock>> ExecuteToolCallsAsync(
        List<ToolUseContent> toolCalls, List<IChatTool> availableTools, ToolContext toolContext,
        CancellationToken cancellationToken)
    {
        var results = new List<ContentBlock>();

        foreach (var toolCall in toolCalls)
        {
            var tool = availableTools.FirstOrDefault(t => t.Name == toolCall.Name);
            string output;

            if (tool is null)
            {
                logger.LogWarning("Unknown tool requested: {ToolName}", toolCall.Name);
                output = $"Error: Unknown tool '{toolCall.Name}'.";
            }
            else
            {
                try
                {
                    output = await tool.ExecuteAsync(toolCall.Input, toolContext, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Tool {ToolName} execution failed", toolCall.Name);
                    output = $"Error executing tool: {ex.Message}";
                }
            }

            results.Add(new ToolResultContent
            {
                ToolUseId = toolCall.Id,
                Content = output,
                IsError = output.StartsWith("Error"),
            });
        }

        return results;
    }
}
