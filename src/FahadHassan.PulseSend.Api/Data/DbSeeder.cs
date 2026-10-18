using FahadHassan.PulseSend.Models;
using Microsoft.EntityFrameworkCore;

namespace FahadHassan.PulseSend.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Customers.AnyAsync())
        {
            db.Customers.AddRange(
                new Customer
                {
                    CreditNumber = "CR-10001",
                    Name = "Max Mustermann",
                    PhoneNumber = "+4915112345678",
                    Amount = 125.50m,
                    DueDate = DateTime.UtcNow.Date.AddDays(7)
                },
                new Customer
                {
                    CreditNumber = "CR-10002",
                    Name = "Erika Musterfrau",
                    PhoneNumber = "+4915112345679",
                    Amount = 280.00m,
                    DueDate = DateTime.UtcNow.Date.AddDays(-3)
                });
        }

        if (!await db.MessageTemplates.AnyAsync())
        {
            db.MessageTemplates.AddRange(
                new MessageTemplate
                {
                    Name = "Erinnerung",
                    Content = "{Creditnumber}\\n\\nSehr geehrte/r {Name},\\n\\nbitte zahlen Sie den Betrag von {amount} EUR bis zum {dueDate}.\\n\\nMit freundlichen Grüßen\\nFahad Hassan | PulseSend"
                },
                new MessageTemplate
                {
                    Name = "Mahnung",
                    Content = "{Creditnumber}\\n\\nSehr geehrte/r {Name},\\n\\nunsere Zahlungserinnerung: Bitte begleichen Sie den offenen Betrag von {amount} EUR bis zum {dueDate}.\\n\\nMit freundlichen Grüßen\\nFahad Hassan | PulseSend"
                },
                new MessageTemplate
                {
                    Name = "Kündigung",
                    Content = "{Creditnumber}\\n\\nSehr geehrte/r {Name},\\n\\nbitte kontaktieren Sie uns bezüglich Ihres offenen Betrags von {amount} EUR. Als Frist ist der {dueDate} vermerkt.\\n\\nMit freundlichen Grüßen\\nFahad Hassan | PulseSend"
                });
        }

        await db.SaveChangesAsync();
    }
}
