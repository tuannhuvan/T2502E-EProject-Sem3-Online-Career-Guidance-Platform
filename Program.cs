using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(); // Add session services

// Register AppDbContext with MySQL (Pomelo)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddIdentity<User, IdentityRole<int>>().AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    await DbSeeder.SeedAsync(dbContext, userManager, roleManager);
    
    Console.WriteLine($"=== DIAGNOSTICS ===");
    Console.WriteLine($"CATEGORIES COUNT: {await dbContext.Categories.CountAsync()}");
    Console.WriteLine($"TESTS COUNT: {await dbContext.Tests.CountAsync()}");
    Console.WriteLine($"QUESTIONS COUNT: {await dbContext.QuestionTests.CountAsync()}");
    Console.WriteLine($"OPTIONS COUNT: {await dbContext.QuestionOptions.CountAsync()}");
    Console.WriteLine($"OPTION-CAREER-PATHS COUNT: {await dbContext.OptionCareerPaths.CountAsync()}");
    Console.WriteLine($"===================");
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