using DocumentationApp.Domain.Entities;
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
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Documentation> Documentations => Set<Documentation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global soft delete filter
        modelBuilder.Entity<User>()
            .HasQueryFilter(x => x.Status != "D");

        modelBuilder.Entity<Application>()
            .HasQueryFilter(x => x.Status != "D");

        modelBuilder.Entity<Documentation>()
            .HasQueryFilter(x => x.Status != "D");
    }
}
