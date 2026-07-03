using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Repository;
using Career_Guidance_Platform.Repository.Interfaces;
using Career_Guidance_Platform.Service;
using Career_Guidance_Platform.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(); // Add session services

// Register AppDbContext with MySQL (Pomelo)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddIdentity<User, IdentityRole<int>>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();;
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IQuestiontestRepository, QuestiontestRepository>();
builder.Services.AddScoped<IQuestiontestService, QuestiontestService>();
var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
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