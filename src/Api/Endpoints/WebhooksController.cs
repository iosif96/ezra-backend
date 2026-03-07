using Application.Domain.Enums;
using Application.Features.Conversations.ProcessIncomingMessage;
using Application.Infrastructure.Services.WhatsApp;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace Api.Endpoints;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ApiControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(IConfiguration configuration, ILogger<WebhooksController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("whatsapp")]
    [AllowAnonymous]
    public IActionResult VerifyWhatsApp(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string verifyToken,
        [FromQuery(Name = "hub.challenge")] string challenge)
    {
        var expectedToken = _configuration["WhatsAppConfiguration:VerifyToken"];

        if (mode == "subscribe" && verifyToken == expectedToken)
            return Ok(challenge);

        _logger.LogWarning("WhatsApp webhook verification failed. Mode: {Mode}", mode);
        return Forbid();
    }

    [HttpPost("whatsapp")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveWhatsApp()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        _logger.LogInformation("WhatsApp webhook received: {Body}", body);

        var payload = JsonConvert.DeserializeObject<WhatsAppWebhookPayload>(body);
        if (payload?.Entry is null || payload.Entry.Count == 0)
            return Ok();

        foreach (var entry in payload.Entry)
        foreach (var change in entry.Changes)
        {
            if (change.Field != "messages")
                continue;

            var messages = change.Value.Messages;
            if (messages is null || messages.Count == 0)
                continue;

            var senderName = change.Value.Contacts?.FirstOrDefault()?.Profile.Name;

            foreach (var msg in messages)
            {
                string? text = null;
                string? mediaId = null;
                string? mediaMimeType = null;

                switch (msg.Type)
                {
                    case "text":
                        text = msg.Text?.Body;
                        break;

                    case "image":
                        mediaId = msg.Image?.Id;
                        mediaMimeType = msg.Image?.MimeType;
                        text = msg.Image?.Caption;
                        break;

                    default:
                        _logger.LogInformation("Unsupported WhatsApp message type: {Type} from {From}", msg.Type, msg.From);
                        continue;
                }

                try
                {
                    await Mediator.Send(new ProcessIncomingMessageCommand(
                        ChannelType: ChannelType.Whatsapp,
                        ChannelId: msg.From,
                        Text: text,
                        MediaId: mediaId,
                        MediaMimeType: mediaMimeType,
                        SenderName: senderName));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process WhatsApp message from {From}", msg.From);
                }
            }
        }

        return Ok();
    }
}
