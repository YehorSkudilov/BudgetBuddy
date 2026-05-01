namespace BudgetBuddyAPI;

using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<BankConnection> BankConnections => Set<BankConnection>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Institution> Institutions => Set<Institution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // -----------------------------
        // USER -> BANK CONNECTIONS
        // -----------------------------
        modelBuilder.Entity<User>()
            .HasIndex(u => u.email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasMany(u => u.bank_connections)
            .WithOne(c => c.user)
            .HasForeignKey(c => c.user_id)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // -----------------------------
        // INSTITUTION -> BANK CONNECTIONS
        // -----------------------------
        modelBuilder.Entity<Institution>()
            .HasMany(i => i.bank_connections)
            .WithOne(c => c.institution)
            .HasForeignKey(c => c.institution_id)
            .IsRequired()
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

        // -----------------------------
        // BANK CONNECTION -> ACCOUNTS (CASCADE FIX)
        // -----------------------------
        modelBuilder.Entity<BankConnection>()
            .HasMany(c => c.accounts)
            .WithOne(a => a.bank_connection)
            .HasForeignKey(a => a.bank_connection_id)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // -----------------------------
        // BANK ACCOUNT OWNED VALUES
        // -----------------------------
        modelBuilder.Entity<BankAccount>()
            .OwnsOne(a => a.balances);

        // -----------------------------
        // BANK ACCOUNT -> TRANSACTIONS (CASCADE FIX)
        // -----------------------------
        modelBuilder.Entity<BankAccount>()
            .HasMany(a => a.transactions)
            .WithOne(t => t.bank_account)
            .HasForeignKey(t => t.bank_account_id)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // -----------------------------
        // TRANSACTION OWNED TYPES
        // -----------------------------
        modelBuilder.Entity<Transaction>()
            .OwnsOne(t => t.personal_finance_category);

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

        // -----------------------------
        // COUNTERPARTIES (SHADOW FK FIXED)
        // -----------------------------
        modelBuilder.Entity<Transaction>()
            .HasMany(t => t.counterparties)
            .WithOne()
            .HasForeignKey("TransactionId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}