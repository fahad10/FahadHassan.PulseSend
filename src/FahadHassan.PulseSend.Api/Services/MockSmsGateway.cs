namespace FahadHassan.PulseSend.Services;

public sealed class MockSmsGateway : ISmsGateway
{
    public Task<SmsSendResult> SendAsync(
        string recipient, string body, string reference, CancellationToken cancellationToken)
    {
        var messageId = $"mock-{Guid.NewGuid():N}";
        var response = System.Text.Json.JsonSerializer.Serialize(new
        {
            id = messageId,
            status = "accepted",
            recipient,
            reference,
            simulated = true
        });

        return Task.FromResult(new SmsSendResult(
            true, "MockAccepted", messageId, null, response));
    }
}
