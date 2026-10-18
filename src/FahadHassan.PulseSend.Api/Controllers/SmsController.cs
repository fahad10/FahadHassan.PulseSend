using FahadHassan.PulseSend.Data;
using FahadHassan.PulseSend.DTOs;
using FahadHassan.PulseSend.Models;
using FahadHassan.PulseSend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FahadHassan.PulseSend.Controllers;

[ApiController]
[Route("api/sms")]
public sealed class SmsController(SmsSendingService sender, AppDbContext db) : ControllerBase
{
    [HttpPost("send")]
    [ProducesResponseType(typeof(SendSmsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Send([FromBody] SendSmsRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.SendAsync(request, cancellationToken);
        if (result.Error is not null)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return StatusCode(result.StatusCode, result.Response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetLog(int id, CancellationToken cancellationToken)
    {
        var log = await db.SmsMessageLogs.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Customer!.CreditNumber,
                x.Template!.Name,
                x.ProviderMessageId,
                x.RecipientPhoneNumber,
                x.MessageContent,
                x.Status,
                x.ErrorMessage,
                x.ProviderResponseJson,
                x.CreatedAtUtc,
                x.UpdatedAtUtc
            }).SingleOrDefaultAsync(cancellationToken);

        return log is null ? NotFound() : Ok(log);
    }

    [HttpGet]
    public async Task<IActionResult> ListLogs(
        [FromQuery] string? creditNumber,
        CancellationToken cancellationToken)
    {
        var query = db.SmsMessageLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(creditNumber))
            query = query.Where(x => x.Customer!.CreditNumber == creditNumber.Trim());

        var logs = await query
            .OrderByDescending(x => x.Id)
            .Take(100)
            .Select(x => new
            {
                x.Id,
                CreditNumber = x.Customer!.CreditNumber,
                TemplateName = x.Template!.Name,
                x.ProviderMessageId,
                x.RecipientPhoneNumber,
                x.Status,
                x.ErrorMessage,
                x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(logs);
    }
}
