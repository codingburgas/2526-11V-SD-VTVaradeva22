using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data.Context;
using TaskManager.Data.Seeding;
using TaskManager.Models.Entities;
using TaskManager.Repositories;
using TaskManager.Services.Implementations;
using TaskManager.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Register the main app database.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    // Configure login, roles, and password rules.
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Tell the app where to send users for login and access errors.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Turn on MVC and add anti-forgery protection for form posts.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Register app services and data access classes.
builder.Services.AddScoped<IBoardRepository, BoardRepository>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IBoardListService, BoardListService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Create roles, demo users, and demo board data on startup.
await DatabaseSeeder.SeedAsync(app);

if (!app.Environment.IsDevelopment())
{
    // Use a friendly error page outside development.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Standard ASP.NET Core middleware pipeline.
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Start the web application.
app.Run();
