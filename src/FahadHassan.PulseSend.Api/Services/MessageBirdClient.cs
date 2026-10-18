using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FahadHassan.PulseSend.Options;
using Microsoft.Extensions.Options;

namespace FahadHassan.PulseSend.Services;

public sealed class MessageBirdClient(
    HttpClient httpClient,
    IOptions<MessageBirdOptions> options) : IMessageBirdClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<SmsSendResult> SendAsync(
        string recipient, string body, string reference, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            return new SmsSendResult(false, "ConfigurationError", null,
                "Bird API key is not configured.", null);

        if (!Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out var baseUri))
            return new SmsSendResult(false, "ConfigurationError", null,
                "Bird BaseUrl is invalid.", null);

        if (string.IsNullOrWhiteSpace(settings.Originator))
            return new SmsSendResult(false, "ConfigurationError", null,
                "Bird sender is not configured.", null);

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUri, "v1/sms/messages"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = JsonContent.Create(new BirdSmsRequest(
            recipient, body, settings.Originator, settings.Category), options: JsonOptions);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new SmsSendResult(false, "ProviderRejected", null,
                    $"Bird returned HTTP {(int)response.StatusCode}.", responseBody);
            }

            string? providerMessageId = null;
            string status = "Accepted";
            try
            {
                using var json = JsonDocument.Parse(responseBody);
                var root = json.RootElement;
                providerMessageId = ReadString(root, "id") ?? ReadString(root, "messageId");
                status = ReadString(root, "status") ?? status;
            }
            catch (JsonException)
            {
                return new SmsSendResult(false, "InvalidProviderResponse", null,
                    "Bird returned a successful HTTP status with an unreadable response.", responseBody);
            }

            return new SmsSendResult(true, status, providerMessageId, null, responseBody);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new SmsSendResult(false, "ProviderTimeout", null,
                "The Bird request timed out. Delivery may be uncertain; do not retry blindly.", null);
        }
        catch (HttpRequestException)
        {
            return new SmsSendResult(false, "ProviderUnavailable", null,
                "Could not reach Bird.", null);
        }
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
            return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private sealed record BirdSmsRequest(
        string To,
        string Text,
        string From,
        string Category);
}
