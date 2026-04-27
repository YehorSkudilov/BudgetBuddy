namespace BudgetBuddyAPI;

using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Text.Json;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<BankConnection> BankConnections => Set<BankConnection>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Institution> Institutions => Set<Institution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Institution>()
            .HasMany(i => i.bank_connections)
            .WithOne(c => c.institution)
            .HasForeignKey(c => c.institution_id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Institution>()
            .Property(i => i.country_codes)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!
            );

        modelBuilder.Entity<Institution>()
            .Property(i => i.products)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!
            );

        modelBuilder.Entity<BankConnection>()
            .HasMany(c => c.accounts)
            .WithOne(a => a.bank_connection)
            .HasForeignKey(a => a.bank_connection_id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BankAccount>()
            .OwnsOne(a => a.balances);

        modelBuilder.Entity<BankAccount>()
            .HasMany(a => a.transactions)
            .WithOne(t => t.bank_account)
            .HasForeignKey(t => t.bank_account_id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .OwnsOne(t => t.personal_finance_category);

        modelBuilder.Entity<Transaction>()
            .HasMany(t => t.counterparties)
            .WithOne()
            .HasForeignKey("TransactionId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.location)
            .WithOne(l => l.transaction)
            .HasForeignKey<TransactionLocation>(l => l.transaction_id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.payment_meta)
            .WithOne(p => p.transaction)
            .HasForeignKey<TransactionPaymentMeta>(p => p.transaction_id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}