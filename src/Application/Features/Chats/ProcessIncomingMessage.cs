using Application.Common.Interfaces;
using Application.Common.Models.Chat;
using Application.Domain.Entities;
using Application.Domain.Enums;
using Application.Features.Chats.Tools;
using Application.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chats.ProcessIncomingMessage;

public record ProcessIncomingMessageCommand(
    ChannelType ChannelType,
    string ChannelId,
    string? Text,
    string? MediaId,
    string? MediaMimeType,
    string? SenderName) : IRequest;

internal sealed class ProcessIncomingMessageHandler(
    ApplicationDbContext context,
    IChatCompletionService completionService,
    IMessagingChannel messagingChannel,
    IEnumerable<IChatTool> tools,
    PromptBuilder promptBuilder,
    IConfiguration configuration,
    ILogger<ProcessIncomingMessageHandler> logger) : IRequestHandler<ProcessIncomingMessageCommand>
{
    private const int MaxToolRounds = 5;

    public async Task Handle(ProcessIncomingMessageCommand command, CancellationToken cancellationToken)
    {
        var conversation = await GetOrCreateConversationAsync(command, cancellationToken);

        var userContent = await BuildUserContentAsync(command, cancellationToken);
        if (userContent.Count == 0)
            return;

        var history = await LoadHistoryAsync(conversation.Id, cancellationToken);

        SaveUserMessage(conversation, command);
        await context.SaveChangesAsync(cancellationToken);

        // Call the LLM with conversation history and tools, running the tool loop if needed
        var (responseText, inputTokens, outputTokens) =
            await GetAssistantResponseAsync(conversation, history, userContent, cancellationToken);

        SaveAssistantMessage(conversation, responseText, inputTokens, outputTokens);
        await context.SaveChangesAsync(cancellationToken);

        await SendReplyAsync(command.ChannelId, responseText, cancellationToken);
    }

    private async Task<List<ContentBlock>> BuildUserContentAsync(
        ProcessIncomingMessageCommand command, CancellationToken cancellationToken)
    {
        var content = new List<ContentBlock>();

        if (command.MediaId is not null)
        {
            var mediaBytes = await messagingChannel.DownloadMediaAsync(command.MediaId, cancellationToken);
            content.Add(new ImageContent
            {
                MediaType = command.MediaMimeType ?? "image/jpeg",
                Base64Data = Convert.ToBase64String(mediaBytes),
            });
        }

        if (!string.IsNullOrWhiteSpace(command.Text))
            content.Add(new TextContent { Text = command.Text });

        return content;
    }

    private void SaveUserMessage(Conversation conversation, ProcessIncomingMessageCommand command)
    {
        conversation.LastMessageOn = DateTime.UtcNow;

        context.Messages.Add(new Message
        {
            ConversationId = conversation.Id,
            Type = MessageType.User,
            MediaType = command.MediaId is not null ? Domain.Enums.MediaType.Image : Domain.Enums.MediaType.Text,
            Content = command.Text,
            MediaUrl = command.MediaId,
        });
    }

    private async Task<(string Text, int InputTokens, int OutputTokens)> GetAssistantResponseAsync(
        Conversation conversation, List<ChatMessageDto> history, List<ContentBlock> userContent,
        CancellationToken cancellationToken)
    {
        var model = configuration["AnthropicConfiguration:Model"] ?? "claude-sonnet-4-5-20250514";
        var toolDefinitions = tools.Select(t => t.ToDefinition()).ToList();

        // Prepend timestamp to user message so the LLM knows when it was sent
        var timestampedContent = new List<ContentBlock>
        {
            new TextContent { Text = $"[{conversation.LastMessageOn:yyyy-MM-dd HH:mm} UTC]" },
        };
        timestampedContent.AddRange(userContent);

        history.Add(new ChatMessageDto
        {
            Role = ChatMessageRole.User,
            Content = timestampedContent,
        });

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

        // Tool loop: let the LLM call tools and feed results back until it produces a final response
        var round = 0;
        while (result.RequiresToolUse && round < MaxToolRounds)
        {
            round++;

            history.Add(new ChatMessageDto
            {
                Role = ChatMessageRole.Assistant,
                Content = result.Content,
            });

            var toolResults = await ExecuteToolCallsAsync(result.GetToolCalls(), conversation.Id, cancellationToken);

            history.Add(new ChatMessageDto
            {
                Role = ChatMessageRole.User,
                Content = toolResults,
            });

            result = await completionService.CompleteAsync(request, cancellationToken);
            totalInputTokens += result.InputTokens;
            totalOutputTokens += result.OutputTokens;
        }

        return (result.GetText() ?? "", totalInputTokens, totalOutputTokens);
    }

    private async Task<List<ContentBlock>> ExecuteToolCallsAsync(
        List<ToolUseContent> toolCalls, int conversationId, CancellationToken cancellationToken)
    {
        var results = new List<ContentBlock>();
        var toolContext = new ToolContext { ConversationId = conversationId };

        foreach (var toolCall in toolCalls)
        {
            var tool = tools.FirstOrDefault(t => t.Name == toolCall.Name);
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

    private void SaveAssistantMessage(
        Conversation conversation, string responseText, int inputTokens, int outputTokens)
    {
        var model = configuration["AnthropicConfiguration:Model"] ?? "claude-sonnet-4-5-20250514";

        context.Messages.Add(new Message
        {
            ConversationId = conversation.Id,
            Type = MessageType.Assistant,
            MediaType = Domain.Enums.MediaType.Text,
            Content = responseText,
            Model = model,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
        });
    }

    private async Task SendReplyAsync(string channelId, string text, CancellationToken cancellationToken)
    {
        var parts = MessageSplitter.SplitIntoMessages(text);

        foreach (var part in parts)
            await messagingChannel.SendTextMessageAsync(channelId, part, cancellationToken);
    }

    private async Task<Conversation> GetOrCreateConversationAsync(
        ProcessIncomingMessageCommand command, CancellationToken cancellationToken)
    {
        var conversation = await context.Conversations
            .FirstOrDefaultAsync(c =>
                c.ChannelType == command.ChannelType &&
                c.ChannelId == command.ChannelId,
                cancellationToken);

        if (conversation is not null)
            return conversation;

        conversation = new Conversation
        {
            ChannelType = command.ChannelType,
            ChannelId = command.ChannelId,
            LastMessageOn = DateTime.UtcNow,
        };

        context.Conversations.Add(conversation);
        await context.SaveChangesAsync(cancellationToken);

        return conversation;
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
                continue;

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
}
