namespace FahadHassan.PulseSend.DTOs;

public sealed record SendSmsResponse(
    int LogId,
    string CreditNumber,
    string TemplateName,
    string Status,
    string? ProviderMessageId,
    string Message);
