using FahadHassan.PulseSend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FahadHassan.PulseSend.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(AppDbContext db) : ControllerBase
{
    [HttpGet("{creditNumber}")]
    public async Task<IActionResult> GetByCreditNumber(string creditNumber, CancellationToken cancellationToken)
    {
        var customer = await db.Customers.AsNoTracking()
            .Where(x => x.CreditNumber == creditNumber)
            .Select(x => new
            {
                x.CreditNumber,
                x.Name,
                x.PhoneNumber,
                x.Amount,
                x.DueDate
            }).SingleOrDefaultAsync(cancellationToken);

        return customer is null ? NotFound() : Ok(customer);
    }
}
