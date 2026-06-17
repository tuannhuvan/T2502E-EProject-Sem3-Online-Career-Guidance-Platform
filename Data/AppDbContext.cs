using Career_Guidance_Platform.Data.SeedData;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Models.SeedData;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<CareerPath> CareerPaths { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>().HasData(CategorySeed.Data);
        modelBuilder.Entity<CareerPath>().HasData(CareerPathSeed.Data);
    }
}