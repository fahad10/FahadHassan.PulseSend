using System.ComponentModel.DataAnnotations;

namespace FahadHassan.PulseSend.DTOs;

public sealed class SendSmsRequest
{
    [Required, MaxLength(64)]
    public string CreditNumber { get; init; } = string.Empty;

    [Required, MaxLength(80)]
    public string TemplateName { get; init; } = string.Empty;
}
