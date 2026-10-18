using FahadHassan.PulseSend.Data;
using FahadHassan.PulseSend.DTOs;
using FahadHassan.PulseSend.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FahadHassan.PulseSend.Tests;

public sealed class SmsSendingServiceTests
{
    [Fact]
    public async Task SendAsync_SavesRenderedMessageAndProviderResponse()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await DbSeeder.SeedAsync(db);
        var gateway = new StubSmsGateway(new SmsSendResult(
            true, "Accepted", "provider-123", null, "{\"id\":\"provider-123\"}"));
        var service = new SmsSendingService(db, new MessageTemplateRenderer(), gateway);

        var result = await service.SendAsync(
            new SendSmsRequest { CreditNumber = "CR-10001", TemplateName = "Erinnerung" },
            CancellationToken.None);

        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Response);
        var log = await db.SmsMessageLogs.SingleAsync();
        Assert.Equal("provider-123", log.ProviderMessageId);
        Assert.Equal("Accepted", log.Status);
        Assert.Contains("Max Mustermann", log.MessageContent);
        Assert.Contains("CR-10001", log.MessageContent);
        Assert.Contains("125,50", log.MessageContent);
        Assert.Equal("{\"id\":\"provider-123\"}", log.ProviderResponseJson);
        Assert.Equal("+4915112345678", log.RecipientPhoneNumber);
    }

    [Fact]
    public async Task SendAsync_UnknownCustomerDoesNotCallGatewayOrCreateLog()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await DbSeeder.SeedAsync(db);
        var gateway = new StubSmsGateway(new SmsSendResult(true, "Accepted", "unused", null, "{}"));
        var service = new SmsSendingService(db, new MessageTemplateRenderer(), gateway);

        var result = await service.SendAsync(
            new SendSmsRequest { CreditNumber = "UNKNOWN-999", TemplateName = "Erinnerung" },
            CancellationToken.None);

        Assert.Equal(404, result.StatusCode);
        Assert.Equal(0, gateway.CallCount);
        Assert.Empty(await db.SmsMessageLogs.ToListAsync());
    }

    [Fact]
    public async Task SendAsync_ProviderFailureIsPersisted()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await DbSeeder.SeedAsync(db);
        var gateway = new StubSmsGateway(new SmsSendResult(
            false, "ProviderRejected", null, "Rejected", "{\"error\":\"rejected\"}"));
        var service = new SmsSendingService(db, new MessageTemplateRenderer(), gateway);

        var result = await service.SendAsync(
            new SendSmsRequest { CreditNumber = "CR-10001", TemplateName = "Mahnung" },
            CancellationToken.None);

        Assert.Equal(502, result.StatusCode);
        var log = await db.SmsMessageLogs.SingleAsync();
        Assert.Equal("ProviderRejected", log.Status);
        Assert.Equal("Rejected", log.ErrorMessage);
        Assert.Equal("{\"error\":\"rejected\"}", log.ProviderResponseJson);
    }

    private sealed class StubSmsGateway(SmsSendResult result) : ISmsGateway
    {
        public int CallCount { get; private set; }

        public Task<SmsSendResult> SendAsync(
            string recipient, string body, string reference, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(result);
        }
    }
}
