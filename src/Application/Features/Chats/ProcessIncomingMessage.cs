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
    IChatCompletionService chatCompletionService,
    IMessagingChannel messagingChannel,
    IEnumerable<IChatTool> chatTools,
    ChatPromptBuilder promptBuilder,
    IConfiguration configuration,
    ILogger<ProcessIncomingMessageHandler> logger) : IRequestHandler<ProcessIncomingMessageCommand>
{
    private const int MaxToolRounds = 5;

    public async Task Handle(ProcessIncomingMessageCommand command, CancellationToken cancellationToken)
    {
        var conversation = await GetOrCreateConversationAsync(command, cancellationToken);

        // Build user content blocks
        var userContent = new List<ContentBlock>();

        if (command.MediaId is not null)
        {
            var mediaBytes = await messagingChannel.DownloadMediaAsync(command.MediaId, cancellationToken);
            userContent.Add(new ImageContent
            {
                MediaType = command.MediaMimeType ?? "image/jpeg",
                Base64Data = Convert.ToBase64String(mediaBytes),
            });
        }

        if (!string.IsNullOrWhiteSpace(command.Text))
            userContent.Add(new TextContent { Text = command.Text });

        if (userContent.Count == 0)
            return;

        // Save user message to DB
        var userMessage = new Message
        {
            ConversationId = conversation.Id,
            Type = MessageType.User,
            MediaType = command.MediaId is not null ? Domain.Enums.MediaType.Image : Domain.Enums.MediaType.Text,
            Content = command.Text,
            MediaUrl = command.MediaId,
        };
        context.Messages.Add(userMessage);

        conversation.LastMessageOn = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        // Load conversation history and build LLM request
        var history = await LoadHistoryAsync(conversation.Id, cancellationToken);
        var systemPrompt = await promptBuilder.BuildAsync(cancellationToken: cancellationToken);
        var model = configuration["AnthropicConfiguration:Model"] ?? "claude-sonnet-4-5-20250514";
        var tools = chatTools.Select(t => t.ToDefinition()).ToList();

        // Add current user message to history
        history.Add(new ChatMessageDto
        {
            Role = ChatMessageRole.User,
            Content = userContent,
        });

        // Call LLM (with tool loop)
        var totalInputTokens = 0;
        var totalOutputTokens = 0;

        var result = await chatCompletionService.CompleteAsync(new ChatCompletionRequest
        {
            Model = model,
            SystemPrompt = systemPrompt,
            Messages = history,
            Tools = tools.Count > 0 ? tools : null,
            MaxTokens = 4096,
        }, cancellationToken);

        totalInputTokens += result.InputTokens;
        totalOutputTokens += result.OutputTokens;

        var toolRounds = 0;
        while (result.RequiresToolUse && toolRounds < MaxToolRounds)
        {
            toolRounds++;

            // Add assistant message with tool calls to history
            history.Add(new ChatMessageDto
            {
                Role = ChatMessageRole.Assistant,
                Content = result.Content,
            });

            // Execute tool calls
            var toolResults = new List<ContentBlock>();
            var toolContext = new ToolContext { ConversationId = conversation.Id };

            foreach (var toolCall in result.GetToolCalls())
            {
                var tool = chatTools.FirstOrDefault(t => t.Name == toolCall.Name);
                string toolOutput;

                if (tool is null)
                {
                    logger.LogWarning("Unknown tool requested: {ToolName}", toolCall.Name);
                    toolOutput = $"Error: Unknown tool '{toolCall.Name}'.";
                }
                else
                {
                    try
                    {
                        toolOutput = await tool.ExecuteAsync(toolCall.Input, toolContext, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Tool {ToolName} execution failed", toolCall.Name);
                        toolOutput = $"Error executing tool: {ex.Message}";
                    }
                }

                toolResults.Add(new ToolResultContent
                {
                    ToolUseId = toolCall.Id,
                    Content = toolOutput,
                    IsError = toolOutput.StartsWith("Error"),
                });
            }

            // Add tool results as user message and call LLM again
            history.Add(new ChatMessageDto
            {
                Role = ChatMessageRole.User,
                Content = toolResults,
            });

            result = await chatCompletionService.CompleteAsync(new ChatCompletionRequest
            {
                Model = model,
                SystemPrompt = systemPrompt,
                Messages = history,
                Tools = tools.Count > 0 ? tools : null,
                MaxTokens = 4096,
            }, cancellationToken);

            totalInputTokens += result.InputTokens;
            totalOutputTokens += result.OutputTokens;
        }

        // Save assistant message to DB
        var responseText = result.GetText() ?? "";

        var assistantMessage = new Message
        {
            ConversationId = conversation.Id,
            Type = MessageType.Assistant,
            MediaType = Domain.Enums.MediaType.Text,
            Content = responseText,
            Model = model,
            InputTokens = totalInputTokens,
            OutputTokens = totalOutputTokens,
        };
        context.Messages.Add(assistantMessage);
        await context.SaveChangesAsync(cancellationToken);

        // Split and send response
        var messageParts = MessageSplitter.SplitIntoMessages(responseText);

        foreach (var part in messageParts)
        {
            await messagingChannel.SendTextMessageAsync(command.ChannelId, part, cancellationToken);
        }
    }

    private async Task<Conversation> GetOrCreateConversationAsync(
        ProcessIncomingMessageCommand command,
        CancellationToken cancellationToken)
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

        // Exclude the message we just added (last one) — we'll add it with full content blocks
        if (messages.Count > 0)
            messages.RemoveAt(messages.Count - 1);

        var history = new List<ChatMessageDto>();

        foreach (var msg in messages)
        {
            var role = msg.Type switch
            {
                MessageType.User => ChatMessageRole.User,
                MessageType.Assistant => ChatMessageRole.Assistant,
                MessageType.System => ChatMessageRole.User, // System context injected as user messages
                _ => ChatMessageRole.User,
            };

            if (string.IsNullOrEmpty(msg.Content))
                continue;

            history.Add(ChatMessageDto.FromText(role, msg.Content));
        }

        return history;
    }
}
