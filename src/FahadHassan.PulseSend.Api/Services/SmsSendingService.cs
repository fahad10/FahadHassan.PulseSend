using FahadHassan.PulseSend.Data;
using FahadHassan.PulseSend.DTOs;
using FahadHassan.PulseSend.Models;
using Microsoft.EntityFrameworkCore;

namespace FahadHassan.PulseSend.Services;

public sealed class SmsSendingService(
    AppDbContext db,
    IMessageTemplateRenderer renderer,
    ISmsGateway gateway)
{
    public async Task<(int StatusCode, SendSmsResponse? Response, string? Error)> SendAsync(
        SendSmsRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CreditNumber) ||
            string.IsNullOrWhiteSpace(request.TemplateName))
            return (400, null, "CreditNumber and TemplateName are required.");

        var customer = await db.Customers.SingleOrDefaultAsync(
            x => x.CreditNumber == request.CreditNumber.Trim(), cancellationToken);
        if (customer is null)
            return (404, null, $"Customer '{request.CreditNumber}' was not found.");

        var template = await db.MessageTemplates.SingleOrDefaultAsync(
            x => x.Name == request.TemplateName.Trim() && x.IsActive, cancellationToken);
        if (template is null)
            return (404, null, $"Active template '{request.TemplateName}' was not found.");

        string message;
        try
        {
            message = renderer.Render(template.Content, customer);
        }
        catch (InvalidOperationException ex)
        {
            return (422, null, ex.Message);
        }

        if (message.Length == 0)
            return (422, null, "Rendered SMS message cannot be empty.");

        var log = new SmsMessageLog
        {
            CustomerId = customer.Id,
            TemplateId = template.Id,
            RecipientPhoneNumber = customer.PhoneNumber,
            MessageContent = message,
            Status = "Pending",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        db.SmsMessageLogs.Add(log);
        await db.SaveChangesAsync(cancellationToken);

        SmsSendResult result;
        try
        {
            result = await gateway.SendAsync(customer.PhoneNumber, message, $"pulsesend-{log.Id}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            log.Status = "GatewayError";
            log.ErrorMessage = "Unexpected SMS gateway failure.";
            log.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return (502, new SendSmsResponse(log.Id, customer.CreditNumber, template.Name,
                log.Status, null, "SMS gateway failed. See the SMS log for details."), null);
        }

        log.Status = result.Status;
        log.ProviderMessageId = result.ProviderMessageId;
        log.ErrorMessage = result.ErrorMessage;
        log.ProviderResponseJson = result.RawResponseJson;
        log.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var response = new SendSmsResponse(
            log.Id,
            customer.CreditNumber,
            template.Name,
            log.Status,
            log.ProviderMessageId,
            result.Accepted
                ? "SMS request accepted. This does not necessarily mean the message was delivered."
                : "SMS request failed. See the SMS log for details.");

        return (result.Accepted ? 200 : 502, response, null);
    }
}
