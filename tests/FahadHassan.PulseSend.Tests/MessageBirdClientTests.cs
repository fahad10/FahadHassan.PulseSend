using System.Net;
using System.Text;
using System.Text.Json;
using FahadHassan.PulseSend.Options;
using FahadHassan.PulseSend.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace FahadHassan.PulseSend.Tests;

public sealed class MessageBirdClientTests
{
    [Fact]
    public async Task SendAsync_UsesBirdEndpointBearerAuthAndJsonPayload()
    {
        string? capturedUri = null;
        string? authorizationScheme = null;
        string? authorizationParameter = null;
        string? capturedBody = null;
        var handler = new StubHttpMessageHandler(async request =>
        {
            capturedUri = request.RequestUri!.ToString();
            authorizationScheme = request.Headers.Authorization!.Scheme;
            authorizationParameter = request.Headers.Authorization.Parameter;
            capturedBody = await request.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent("{\"id\":\"bird-msg-123\",\"status\":\"accepted\"}", Encoding.UTF8, "application/json")
            };
        });
        using var httpClient = new HttpClient(handler);
        var settings = Microsoft.Extensions.Options.Options.Create(new MessageBirdOptions
        {
            BaseUrl = "https://eu1.platform.bird.com/",
            ApiKey = "test-key",
            Originator = "ApprovedSender",
            Category = "transactional"
        });
        var client = new MessageBirdClient(httpClient, settings);

        var result = await client.SendAsync("+4915112345678", "Test message", "pulsesend-1", CancellationToken.None);

        Assert.True(result.Accepted);
        Assert.Equal("bird-msg-123", result.ProviderMessageId);
        Assert.Equal("https://eu1.platform.bird.com/v1/sms/messages", capturedUri);
        Assert.Equal("Bearer", authorizationScheme);
        Assert.Equal("test-key", authorizationParameter);
        using var body = JsonDocument.Parse(capturedBody!);
        Assert.Equal("+4915112345678", body.RootElement.GetProperty("to").GetString());
        Assert.Equal("Test message", body.RootElement.GetProperty("text").GetString());
        Assert.Equal("ApprovedSender", body.RootElement.GetProperty("from").GetString());
        Assert.Equal("transactional", body.RootElement.GetProperty("category").GetString());
    }

    [Fact]
    public async Task SendAsync_WithoutApiKeyReturnsConfigurationError()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(_ =>
            throw new InvalidOperationException("The HTTP handler should not be called.")));
        var client = new MessageBirdClient(
    httpClient,
    Microsoft.Extensions.Options.Options.Create(new MessageBirdOptions()));

        var result = await client.SendAsync("+4915112345678", "Test", "ref-1", CancellationToken.None);

        Assert.False(result.Accepted);
        Assert.Equal("ConfigurationError", result.Status);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) => handler(request);
    }
}
