using KX_Project.Models;
using KX_Project.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;
using DotNetEnv;
using KX_Project.Services;
using QuestPDF.Infrastructure;

// Ensure the application runs from the project root so it can find the wwwroot folder
var currentDir = Directory.GetCurrentDirectory();
if (!Directory.Exists(Path.Combine(currentDir, "wwwroot")))
{
    // If run directly from bin\Debug\net..., the project root is 3 levels up
    var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
    if (Directory.Exists(Path.Combine(projectDir, "wwwroot")))
    {
        Directory.SetCurrentDirectory(projectDir);
    }
}

// Load environment variables after setting correct directory
Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? "MISSING_CLIENT_ID";
        options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? "MISSING_CLIENT_SECRET";
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

// Thêm cấu hình cho Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

QuestPDF.Settings.License = LicenseType.Community;

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await RoleSeeder.SeedRolesAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "KoraStore API v1");
    // Tuỳ chọn: c.RoutePrefix = string.Empty; để Swagger UI ở trang chủ, nhưng mình để mặc định ở /swagger
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");


app.Run();
