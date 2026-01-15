using DocumentationApp.Domain.Entities;
using DocumentationAppApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentationAppApi.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserType> UserTypes => Set<UserType>();
    public DbSet<App> Applications => Set<App>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();


    /// <summary>
    /// Configures the EF Core model and applies global soft-delete filters for entity types.
    /// </summary>
    /// <remarks>
    /// Calls the base implementation and sets query filters so entities with Status equal to "D"
    /// are excluded from queries for User, App, and Document types.
    /// </remarks>
    /// <param name="modelBuilder">The ModelBuilder used to configure entity mappings and query filters.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global soft delete filter
        modelBuilder.Entity<User>()
            .HasQueryFilter(x => x.Status != "D");

        modelBuilder.Entity<App>()
            .HasQueryFilter(x => x.Status != "D");

        modelBuilder.Entity<Document>()
            .HasQueryFilter(x => x.Status != "D");
    }
}