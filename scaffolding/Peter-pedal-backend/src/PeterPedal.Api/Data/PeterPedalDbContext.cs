using Microsoft.EntityFrameworkCore;
using PeterPedal.Api.Models;

namespace PeterPedal.Api.Data;

public class PeterPedalDbContext : DbContext
{
    public PeterPedalDbContext(DbContextOptions<PeterPedalDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SparePart> SpareParts => Set<SparePart>();
    public DbSet<RepairCase> RepairCases => Set<RepairCase>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Many-to-many: a case has many parts, and the same part can be used on many cases.
        // SparePart has no list of cases, so EF Core needs to be told this explicitly.
        modelBuilder.Entity<RepairCase>().HasMany(repairCase => repairCase.Parts).WithMany();

        // A customer's login belongs to the customer and is deleted with them. No navigation property,
        // so User (with its password hash) stays out of REST and GraphQL.
        modelBuilder.Entity<User>().HasOne<Customer>().WithMany().HasForeignKey(u => u.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
    }
}
