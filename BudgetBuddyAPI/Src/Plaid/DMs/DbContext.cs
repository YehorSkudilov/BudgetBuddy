namespace BudgetBuddyAPI;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<BankConnection> BankConnections => Set<BankConnection>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.BankConnection)
            .WithMany()
            .HasForeignKey(t => t.BankConnectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}