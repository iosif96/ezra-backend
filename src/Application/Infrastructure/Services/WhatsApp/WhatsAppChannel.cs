using System.Text;

using Application.Common.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Application.Infrastructure.Services.WhatsApp;

public class WhatsAppChannel : IMessagingChannel
{
    private readonly HttpClient _httpClient;
    private readonly string _phoneNumberId;
    private readonly string _accessToken;
    private readonly string _apiVersion;
    private readonly ILogger<WhatsAppChannel> _logger;

    public WhatsAppChannel(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WhatsAppChannel> logger)
    {
        _httpClient = httpClient;
        _phoneNumberId = configuration["WhatsAppConfiguration:PhoneNumberId"]
            ?? throw new InvalidOperationException("WhatsApp PhoneNumberId not configured");
        _accessToken = configuration["WhatsAppConfiguration:AccessToken"]
            ?? throw new InvalidOperationException("WhatsApp AccessToken not configured");
        _apiVersion = configuration["WhatsAppConfiguration:ApiVersion"] ?? "v21.0";
        _logger = logger;
    }

    public async Task SendTextMessageAsync(string channelId, string text, CancellationToken cancellationToken = default)
    {
        var url = $"https://graph.facebook.com/{_apiVersion}/{_phoneNumberId}/messages";

        var payload = new WhatsAppSendMessageRequest
        {
            To = channelId,
            Text = new WhatsAppSendText { Body = text },
        };

        var json = JsonConvert.SerializeObject(payload);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {_accessToken}");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("WhatsApp send failed {StatusCode}: {Error}", response.StatusCode, error);
            throw new HttpRequestException($"WhatsApp send failed ({response.StatusCode}): {error}");
        }
    }

    public async Task<byte[]> DownloadMediaAsync(string mediaId, CancellationToken cancellationToken = default)
    {
        // Step 1: Get the media URL
        var metadataUrl = $"https://graph.facebook.com/{_apiVersion}/{mediaId}";

        using var metadataRequest = new HttpRequestMessage(HttpMethod.Get, metadataUrl);
        metadataRequest.Headers.Add("Authorization", $"Bearer {_accessToken}");

        using var metadataResponse = await _httpClient.SendAsync(metadataRequest, cancellationToken);
        metadataResponse.EnsureSuccessStatusCode();

        var metadataJson = await metadataResponse.Content.ReadAsStringAsync(cancellationToken);
        var media = JsonConvert.DeserializeObject<WhatsAppMediaResponse>(metadataJson)
            ?? throw new InvalidOperationException("Failed to get media URL");

        // Step 2: Download the actual media
        using var downloadRequest = new HttpRequestMessage(HttpMethod.Get, media.Url);
        downloadRequest.Headers.Add("Authorization", $"Bearer {_accessToken}");

        using var downloadResponse = await _httpClient.SendAsync(downloadRequest, cancellationToken);
        downloadResponse.EnsureSuccessStatusCode();

        return await downloadResponse.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
