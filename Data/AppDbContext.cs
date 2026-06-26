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
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<FaqItem> FaqItems { get; set; }
        public DbSet<NewsArticle> NewsArticles { get; set; }
        public DbSet<CareerEvent> CareerEvents { get; set; }
        public DbSet<CommunityPost> CommunityPosts { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Resume> Resumes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Entities already have [Table]/[Column] attributes; add extra mapping if needed here.
        }
    }
}