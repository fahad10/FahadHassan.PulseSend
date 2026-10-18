using System.ComponentModel.DataAnnotations;

namespace FahadHassan.PulseSend.Models;

public sealed class Customer
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string CreditNumber { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }

    public ICollection<SmsMessageLog> SmsLogs { get; set; } = new List<SmsMessageLog>();
}
