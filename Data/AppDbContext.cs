using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Career_Guidance_Platform.Models;

namespace Career_Guidance_Platform.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<Test> Tests { get; set; }
        public DbSet<QuestionTest> QuestionTests { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<TestAnswer> TestAnswers { get; set; }
        public DbSet<OptionCareerPath> OptionCareerPaths { get; set; }
        public DbSet<CareerPath> CareerPaths { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Entities already have [Table]/[Column] attributes; add extra mapping if needed here.
        }
    }
}