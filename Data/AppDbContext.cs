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
        
        
        
        public DbSet<UserCourseProgress> UserCourseProgresses { get; set; }
        
        public DbSet<CareerPathCourse> CareerPathCourses { get; set; }
        public DbSet<Resume> Resumes { get; set; }

        public DbSet<Skill> Skills { get; set; }
        public DbSet<CareerPathSkill> CareerPathSkills { get; set; }
        public DbSet<UserSkill> UserSkills { get; set; }
        public DbSet<SuccessStory> SuccessStories { get; set; }
        public DbSet<MentorProfile> MentorProfiles { get; set; }
        public DbSet<MentorReview> MentorReviews { get; set; }
        public DbSet<MentorshipRequest> MentorshipRequests { get; set; }
        public DbSet<MentorshipMeeting> MentorshipMeetings { get; set; }
        public DbSet<MentorshipMessage> MentorshipMessages { get; set; }
        public DbSet<GroupMentoringSession> GroupMentoringSessions { get; set; }
        public DbSet<GroupMentoringRegistration> GroupMentoringRegistrations { get; set; }
        public DbSet<SavedJob> SavedJobs { get; set; }
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<ApplicationReminder> ApplicationReminders { get; set; }
        public DbSet<EmployerReview> EmployerReviews { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<GoalMilestone> GoalMilestones { get; set; }
        public DbSet<CareerStage> CareerStages { get; set; }
        public DbSet<CareerStageSkill> CareerStageSkills { get; set; }
        public DbSet<ResumeTemplate> ResumeTemplates { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }
        public DbSet<CommunityComment> CommunityComments { get; set; }
        public DbSet<PeerConnection> PeerConnections { get; set; }
        public DbSet<TestResultScore> TestResultScores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CareerPathCourse>()
                .HasOne(c => c.CareerPath)
                .WithMany()
                .HasForeignKey(c => c.CareerPathId);

            // CareerPath self-referencing hierarchy
            modelBuilder.Entity<CareerPath>()
                .HasOne(cp => cp.ParentPath)
                .WithMany(cp => cp.ChildPaths)
                .HasForeignKey(cp => cp.ParentPathId)
                .OnDelete(DeleteBehavior.Restrict);

            // CareerPathSkill composite key
            modelBuilder.Entity<CareerPathSkill>()
                .HasKey(cps => new { cps.CareerPathId, cps.SkillId });

            // CareerStageSkill composite key
            modelBuilder.Entity<CareerStageSkill>()
                .HasKey(css => new { css.CareerStageId, css.SkillId });

            // UserSkill composite key
            modelBuilder.Entity<UserSkill>()
                .HasKey(us => new { us.UserId, us.SkillId });

            // GroupMentoringRegistration composite key
            modelBuilder.Entity<GroupMentoringRegistration>()
                .HasKey(gmr => new { gmr.SessionId, gmr.StudentId });

            // SavedJob composite key
            modelBuilder.Entity<SavedJob>()
                .HasKey(sj => new { sj.UserId, sj.JobPostingId });

            // EventRegistration composite key
            modelBuilder.Entity<EventRegistration>()
                .HasKey(er => new { er.EventId, er.UserId });

            // PeerConnection composite key
            modelBuilder.Entity<PeerConnection>()
                .HasKey(pc => new { pc.RequesterId, pc.ReceiverId });

            // MentorProfile 1-1 with User
            modelBuilder.Entity<MentorProfile>()
                .HasKey(mp => mp.UserId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.MentorProfile)
                .WithOne(mp => mp.User)
                .HasForeignKey<MentorProfile>(mp => mp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // MentorshipRequest navigation configuration (multiple FKs to User)
            modelBuilder.Entity<MentorshipRequest>()
                .HasOne(mr => mr.Mentee)
                .WithMany(u => u.MenteeRequests)
                .HasForeignKey(mr => mr.MenteeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MentorshipRequest>()
                .HasOne(mr => mr.Mentor)
                .WithMany(u => u.MentorRequests)
                .HasForeignKey(mr => mr.MentorId)
                .OnDelete(DeleteBehavior.Restrict);

            // MentorshipMeeting navigation configuration (multiple FKs to User)
            modelBuilder.Entity<MentorshipMeeting>()
                .HasOne(mm => mm.Mentee)
                .WithMany(u => u.MenteeMeetings)
                .HasForeignKey(mm => mm.MenteeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MentorshipMeeting>()
                .HasOne(mm => mm.Mentor)
                .WithMany(u => u.MentorMeetings)
                .HasForeignKey(mm => mm.MentorId)
                .OnDelete(DeleteBehavior.Restrict);

            // MentorshipMessage navigation configuration (multiple FKs to User)
            modelBuilder.Entity<MentorshipMessage>()
                .HasOne(mm => mm.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(mm => mm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MentorshipMessage>()
                .HasOne(mm => mm.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(mm => mm.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // MentorReview relationship mappings
            modelBuilder.Entity<MentorReview>()
                .HasOne(mr => mr.Mentor)
                .WithMany()
                .HasForeignKey(mr => mr.MentorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MentorReview>()
                .HasOne(mr => mr.Mentee)
                .WithMany()
                .HasForeignKey(mr => mr.MenteeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MentorReview>()
                .HasOne(mr => mr.Meeting)
                .WithMany()
                .HasForeignKey(mr => mr.MeetingId)
                .OnDelete(DeleteBehavior.Restrict);

            // PeerConnection navigation configuration (multiple FKs to User)
            modelBuilder.Entity<PeerConnection>()
                .HasOne(pc => pc.Requester)
                .WithMany(u => u.PeerConnectionsSent)
                .HasForeignKey(pc => pc.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PeerConnection>()
                .HasOne(pc => pc.Receiver)
                .WithMany(u => u.PeerConnectionsReceived)
                .HasForeignKey(pc => pc.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // JobApplication navigation configuration
            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.User)
                .WithMany(u => u.JobApplications)
                .HasForeignKey(ja => ja.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.JobPosting)
                .WithMany(jp => jp.JobApplications)
                .HasForeignKey(ja => ja.JobPostingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.Resume)
                .WithMany()
                .HasForeignKey(ja => ja.ResumeId)
                .OnDelete(DeleteBehavior.Restrict);

            // EmployerReview navigation configuration
            modelBuilder.Entity<EmployerReview>()
                .HasOne(er => er.User)
                .WithMany(u => u.EmployerReviews)
                .HasForeignKey(er => er.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // CommunityPost author foreign key
            modelBuilder.Entity<CommunityPost>()
                .HasOne(cp => cp.Author)
                .WithMany(u => u.CommunityPosts)
                .HasForeignKey(cp => cp.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // CommunityComment relations
            modelBuilder.Entity<CommunityComment>()
                .HasOne(cc => cc.CommunityPost)
                .WithMany(cp => cp.Comments)
                .HasForeignKey(cc => cc.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommunityComment>()
                .HasOne(cc => cc.Author)
                .WithMany(u => u.CommunityComments)
                .HasForeignKey(cc => cc.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Goal relations (cascade goal delete, restrict path delete)
            modelBuilder.Entity<Goal>()
                .HasOne(g => g.Student)
                .WithMany(u => u.Goals)
                .HasForeignKey(g => g.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Goal>()
                .HasOne(g => g.CareerPath)
                .WithMany(cp => cp.Goals)
                .HasForeignKey(g => g.CareerPathId)
                .OnDelete(DeleteBehavior.Restrict);

            // Resource self-referencing hierarchy
            modelBuilder.Entity<Resource>()
                .HasOne(r => r.ParentResource)
                .WithMany(p => p.ChildResources)
                .HasForeignKey(r => r.ParentResourceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Resource>()
                .HasOne(r => r.Skill)
                .WithMany(s => s.Resources)
                .HasForeignKey(r => r.SkillId)
                .OnDelete(DeleteBehavior.Restrict);

            // GoalMilestone cascade delete
            modelBuilder.Entity<GoalMilestone>()
                .HasOne(gm => gm.Goal)
                .WithMany(g => g.GoalMilestones)
                .HasForeignKey(gm => gm.GoalId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}