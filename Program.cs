using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Repository;
using Career_Guidance_Platform.Repository.Interfaces;
using Career_Guidance_Platform.SeedData;
using Career_Guidance_Platform.Service;
using Career_Guidance_Platform.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews(options =>
{
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSignalR();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    options.AddFixedWindowLimiter("loginPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
    });

    options.AddFixedWindowLimiter("forgotPasswordPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(5);
        opt.PermitLimit = 3;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
    });
});

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<VietnameseIdentityErrorDescriber>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context => {
        if (context.Request.Path.StartsWithSegments("/Admin")) {
            context.Response.Redirect("/Account/AdminLogin" + context.Request.QueryString);
        } else {
            context.Response.Redirect("/Account/Login" + context.Request.QueryString);
        }
        return System.Threading.Tasks.Task.CompletedTask;
    };
});


// Repositories
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IQuestionUserRepository, QuestionUserRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IQuestionUserService, QuestionUserService>();
builder.Services.AddSingleton<PresenceTracker>();

var app = builder.Build();

// Run migrations and seed data
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
            var migrations = new[]
            {
                "20260616195539_InitDatabase",
                "20260617071207_RenameQuestionTypeNameToTitle",
                "20260617080422_FixQuestionTable",
                "20260618112323_careertest",
                "20260618113000_Careertests",
                "20260618145308_UserAnswer",
                "20260618145831_updateUserAnswer",
                "20260619010439_userAnswerupdate",
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
                "20260714081724_AddPaymentHistory",
                "20260722044410_AddUserFieldsAndNotification",
                "20260722054421_RemoveEmployerReviewsTable",
                "20260722062012_AddIsVipToEventRegistration"
            };

            foreach (var migration in migrations)
            {
                dbContext.Database.ExecuteSql($@"
                    INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
                    VALUES ({migration}, '8.0.0');
                ");
            }

            // Manually run ALTER statements in try-catch to ensure columns/tables exist
            try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE `resume_templates` ADD `is_premium` tinyint(1) NOT NULL DEFAULT FALSE;"); } catch {}
            try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE `career_events` ADD `max_participants` int NOT NULL DEFAULT 0;"); } catch {}
            try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE `AspNetUsers` ADD `avatar_url` longtext CHARACTER SET utf8mb4 NULL;"); } catch {}
            try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE `AspNetUsers` ADD `experience` longtext CHARACTER SET utf8mb4 NULL;"); } catch {}
            try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE `AspNetUsers` ADD `headline` longtext CHARACTER SET utf8mb4 NULL;"); } catch {}
            try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE `AspNetUsers` ADD `major` longtext CHARACTER SET utf8mb4 NULL;"); } catch {}
            try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE `AspNetUsers` ADD `school` longtext CHARACTER SET utf8mb4 NULL;"); } catch {}
            try {
                dbContext.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS `notifications` (
                        `id` int NOT NULL AUTO_INCREMENT,
                        `user_id` int NOT NULL,
                        `message` longtext CHARACTER SET utf8mb4 NOT NULL,
                        `is_read` tinyint(1) NOT NULL,
                        `created_at` datetime(6) NOT NULL,
                        CONSTRAINT `PK_notifications` PRIMARY KEY (`id`),
                        CONSTRAINT `FK_notifications_AspNetUsers_user_id` FOREIGN KEY (`user_id`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
                    ) CHARACTER SET=utf8mb4;
                ");
            } catch {}
            try { dbContext.Database.ExecuteSqlRaw("CREATE INDEX `IX_notifications_user_id` ON `notifications` (`user_id`);"); } catch {}
            try { dbContext.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS `employer_reviews`;"); } catch {}
            try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE `event_registrations` ADD `is_vip` tinyint(1) NOT NULL DEFAULT FALSE;"); } catch {}
            try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE `mentor_reviews` ADD `reply_comment` longtext CHARACTER SET utf8mb4 NULL;"); } catch {}
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
app.UseRateLimiter();
app.UseSession();
app.UseAuthentication();
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
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<Career_Guidance_Platform.Hubs.ChatHub>("/chatHub");
app.MapHub<Career_Guidance_Platform.Hubs.PresenceAndNotificationHub>("/hubs/presenceNotification");
app.MapHub<Career_Guidance_Platform.Hubs.NotificationHub>("/notificationHub");

app.Run();