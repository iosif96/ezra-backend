using Application.Domain.Enums;
using Application.Features.Chats.ProcessIncomingMessage;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

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

        JObject payload;
        try
        {
            payload = JObject.Parse(body);
        }
        catch
        {
            return BadRequest();
        }

        // Only process message events
        var messages = payload.SelectTokens("$.entry[*].changes[?(@.field=='messages')].value.messages[*]");
        var contacts = payload.SelectTokens("$.entry[*].changes[?(@.field=='messages')].value.contacts[*]");

        var contactName = contacts.FirstOrDefault()?["profile"]?["name"]?.Value<string>();

        foreach (var msg in messages)
        {
            var from = msg["from"]?.Value<string>();
            var type = msg["type"]?.Value<string>();

            if (from is null || type is null)
                continue;

            // Skip status updates and unsupported types for now
            string? text = null;
            string? mediaId = null;
            string? mediaMimeType = null;

            switch (type)
            {
                case "text":
                    text = msg["text"]?["body"]?.Value<string>();
                    break;

                case "image":
                    mediaId = msg["image"]?["id"]?.Value<string>();
                    mediaMimeType = msg["image"]?["mime_type"]?.Value<string>();
                    text = msg["image"]?["caption"]?.Value<string>();
                    break;

                default:
                    _logger.LogInformation("Unsupported WhatsApp message type: {Type} from {From}", type, from);
                    continue;
            }

            try
            {
                await Mediator.Send(new ProcessIncomingMessageCommand(
                    ChannelType: ChannelType.Whatsapp,
                    ChannelId: from,
                    Text: text,
                    MediaId: mediaId,
                    MediaMimeType: mediaMimeType,
                    SenderName: contactName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process WhatsApp message from {From}", from);
            }
        }

        // Always return 200 to WhatsApp to prevent retries
        return Ok();
    }
}
