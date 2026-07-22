using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(); // Add session services

// Register AppDbContext with MySQL (Pomelo)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<Career_Guidance_Platform.Service.VietnameseIdentityErrorDescriber>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        // Ensure __EFMigrationsHistory exists
        dbContext.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
                `MigrationId` varchar(150) NOT NULL,
                `ProductVersion` varchar(32) NOT NULL,
                PRIMARY KEY (`MigrationId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");

        // Check if a core table like AspNetRoles already exists
        bool tableExists = false;
        var conn = dbContext.Database.GetDbConnection();
        bool wasClosed = conn.State == System.Data.ConnectionState.Closed;
        if (wasClosed)
        {
            conn.Open();
        }
        try
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AspNetRoles';";
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                tableExists = count > 0;
            }
        }
        finally
        {
            if (wasClosed)
            {
                conn.Close();
            }
        }

        if (tableExists)
        {
            // Pre-populate __EFMigrationsHistory with known migrations if they aren't there yet
            var migrations = new[]
            {
                "20260623201748_InitialCreate",
                "20260624040356_AddDateTakenToTestResult",
                "20260624162724_AddTeamMembersTable",
                "20260624164219_AddCommunityNewsEvents",
                "20260624170853_UpdateTestResultAndResources",
                "20260624194048_AddTestResultDetails",
                "20260625130511_AddCareerPathCourses",
                "20260626084840_AddResumeTable",
                "20260626133501_ExpandDatabaseSchema",
                "20260630164047_UpdateSchemaForDetailedAspects",
                "20260702112800_AddGoalUserSkillFields",
                "20260703062347_AddMentorshipUpdates",
                "20260714071928_AddPremiumFieldsToUser",
                "20260714081724_AddPaymentHistory"
            };

            foreach (var migration in migrations)
            {
                dbContext.Database.ExecuteSqlRaw($@"
                    INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
                    VALUES ('{migration}', '8.0.0');
                ");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARNING] Failed to pre-seed EF migration history table: {ex.Message}");
    }

    dbContext.Database.Migrate();

    // Ensure user_course_progress table is created (fixes EF Core migration desync on MySQL)
    var createTableSql = @"
        CREATE TABLE IF NOT EXISTS user_course_progress (
            Id INT AUTO_INCREMENT PRIMARY KEY,
            UserId INT NOT NULL,
            CourseId INT NOT NULL,
            StartDate DATETIME(6) NOT NULL,
            DeadlineDate DATETIME(6) NOT NULL,
            ProgressPercent INT NOT NULL,
            Status LONGTEXT NOT NULL,
            TestPassed TINYINT(1) NOT NULL,
            TestScore DOUBLE NOT NULL,
            CONSTRAINT FK_user_course_progress_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
            CONSTRAINT FK_user_course_progress_career_path_courses_CourseId FOREIGN KEY (CourseId) REFERENCES career_path_courses(id) ON DELETE CASCADE
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
    ";
    Console.WriteLine("[DEBUG] Running ExecuteSqlRaw for user_course_progress...");
    dbContext.Database.ExecuteSqlRaw(createTableSql);
    Console.WriteLine("[DEBUG] Table user_course_progress check/creation complete.");

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    await DbSeeder.SeedAsync(dbContext, userManager, roleManager);

    var allTests = dbContext.Tests.Include(t => t.QuestionTests).ToList();
    Console.WriteLine($"[DEBUG] Total tests in DB: {allTests.Count}");
    foreach (var t in allTests)
    {
        Console.WriteLine($"[DEBUG] Test ID: {t.Id}, Title: '{t.Title}', Status: {t.Status}, Question count: {t.QuestionTests.Count}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); // Use session middleware

app.UseAuthentication(); // ensure Authentication is registered if you use it
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "admin",
    defaults: new
    {
        controller = "Admin",
        action = "Index"
    });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();