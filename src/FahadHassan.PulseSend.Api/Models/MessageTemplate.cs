using System.ComponentModel.DataAnnotations;

namespace FahadHassan.PulseSend.Models;

public sealed class MessageTemplate
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
