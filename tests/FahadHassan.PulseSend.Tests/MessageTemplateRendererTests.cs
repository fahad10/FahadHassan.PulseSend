using FahadHassan.PulseSend.Models;
using FahadHassan.PulseSend.Services;
using Xunit;

namespace FahadHassan.PulseSend.Tests;

public sealed class MessageTemplateRendererTests
{
    private readonly MessageTemplateRenderer _renderer = new();

    [Fact]
    public void Render_ReplacesCustomerPlaceholders()
    {
        var customer = new Customer
        {
            CreditNumber = "CR-123",
            Name = "Max Mustermann",
            PhoneNumber = "+4915112345678",
            Amount = 125.5m,
            DueDate = new DateTime(2026, 11, 2)
        };

        var result = _renderer.Render(
            "{Creditnumber}|{Name}|{amount}|{dueDate}", customer);

        Assert.Equal("CR-123|Max Mustermann|125,50|02.11.2026", result);
    }

    [Fact]
    public void Render_ThrowsForUnsupportedPlaceholder()
    {
        var customer = new Customer
        {
            CreditNumber = "CR-123",
            Name = "Max",
            PhoneNumber = "+4915112345678",
            Amount = 1,
            DueDate = DateTime.Today
        };

        Assert.Throws<InvalidOperationException>(() =>
            _renderer.Render("Hello {Unknown}", customer));
    }
}
