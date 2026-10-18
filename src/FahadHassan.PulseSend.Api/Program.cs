using FahadHassan.PulseSend.Data;
using FahadHassan.PulseSend.Options;
using FahadHassan.PulseSend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MessageBirdOptions>(
    builder.Configuration.GetSection(MessageBirdOptions.SectionName));

var provider = builder.Configuration["Database:Provider"] ?? "Sqlite";
if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    var connectionString = builder.Configuration.GetConnectionString("SqlServer")
        ?? throw new InvalidOperationException("ConnectionStrings:SqlServer is required when Database:Provider=SqlServer.");
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("Sqlite")
        ?? "Data Source=pulsesend.db";
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
}

builder.Services.AddScoped<IMessageTemplateRenderer, MessageTemplateRenderer>();
builder.Services.AddHttpClient<IMessageBirdClient, MessageBirdClient>();
builder.Services.AddScoped<ISmsGateway>(services =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    var mode = configuration["Sms:Mode"] ?? "Mock";
    return mode.Equals("MessageBird", StringComparison.OrdinalIgnoreCase)
        ? services.GetRequiredService<IMessageBirdClient>()
        : services.GetRequiredService<MockSmsGateway>();
});
builder.Services.AddScoped<MockSmsGateway>();
builder.Services.AddScoped<SmsSendingService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DbSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

public partial class Program { }
