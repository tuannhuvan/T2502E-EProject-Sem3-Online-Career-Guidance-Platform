using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    // Career Tests
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<OptionCareerPath> OptionCareerPaths { get; set; }
    public DbSet<CareerPath> CareerPaths { get; set; }
    public DbSet<QuestionType> QuestionTypes { get; set; }
    public DbSet<CareerTest> CareerTests { get; set; }
    
    public DbSet<Category> Categories { get; set; }
    public DbSet<AssessmentResult> AssessmentResults { get; set; }

    public DbSet<UserAnswer> UserAnswers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Question - Option (1-N)
        modelBuilder.Entity<Question>()
            .HasMany(q => q.QuestionOptions)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId);

        // Option - CareerPath (N-N via table)
        modelBuilder.Entity<OptionCareerPath>()
            .HasOne(x => x.QuestionOption)
            .WithMany(o => o.OptionCareerPaths)
            .HasForeignKey(x => x.OptionId);

        modelBuilder.Entity<OptionCareerPath>()
            .HasOne(x => x.CareerPath)
            .WithMany(c => c.OptionCareerPaths)
            .HasForeignKey(x => x.CareerPathId);
    }
}  
