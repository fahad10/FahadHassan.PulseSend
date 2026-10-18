namespace FahadHassan.PulseSend.Services;

public sealed record SmsSendResult(
    bool Accepted,
    string Status,
    string? ProviderMessageId,
    string? ErrorMessage,
    string? RawResponseJson);

public interface ISmsGateway
{
    Task<SmsSendResult> SendAsync(string recipient, string body, string reference, CancellationToken cancellationToken);
}

public interface IMessageBirdClient : ISmsGateway { }

public interface IMessageTemplateRenderer
{
    string Render(string template, FahadHassan.PulseSend.Models.Customer customer);
}
