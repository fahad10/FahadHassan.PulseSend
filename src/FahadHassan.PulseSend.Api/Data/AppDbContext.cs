using FahadHassan.PulseSend.Models;
using Microsoft.EntityFrameworkCore;

namespace FahadHassan.PulseSend.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
    public DbSet<SmsMessageLog> SmsMessageLogs => Set<SmsMessageLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .HasIndex(x => x.CreditNumber).IsUnique();

        modelBuilder.Entity<Customer>()
            .Property(x => x.Amount).HasPrecision(18, 2);

        modelBuilder.Entity<SmsMessageLog>()
            .HasOne(x => x.Customer)
            .WithMany(x => x.SmsLogs)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SmsMessageLog>()
            .HasOne(x => x.Template)
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MessageTemplate>()
            .HasIndex(x => x.Name).IsUnique();
    }
}
