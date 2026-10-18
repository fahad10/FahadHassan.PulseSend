using System.ComponentModel.DataAnnotations;

namespace FahadHassan.PulseSend.Models;

public sealed class SmsMessageLog
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int TemplateId { get; set; }
    public MessageTemplate? Template { get; set; }

    [MaxLength(128)]
    public string? ProviderMessageId { get; set; }

    [Required, MaxLength(32)]
    public string RecipientPhoneNumber { get; set; } = string.Empty;

    [Required, MaxLength(4000)]
    public string MessageContent { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string Status { get; set; } = "Pending";

    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    public string? ProviderResponseJson { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
