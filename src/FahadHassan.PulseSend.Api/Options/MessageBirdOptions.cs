namespace FahadHassan.PulseSend.Options;

public sealed class MessageBirdOptions
{
    public const string SectionName = "MessageBird";
    public string BaseUrl { get; set; } = "https://eu1.platform.bird.com/";
    public string Originator { get; set; } = string.Empty;
    public string Category { get; set; } = "transactional";
    public string ApiKey { get; set; } = string.Empty;
}
