using FahadHassan.PulseSend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FahadHassan.PulseSend.Controllers;

[ApiController]
[Route("api/templates")]
public sealed class TemplatesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTemplates(CancellationToken cancellationToken)
    {
        var templates = await db.MessageTemplates.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Content, x.IsActive })
            .ToListAsync(cancellationToken);
        return Ok(templates);
    }
}
