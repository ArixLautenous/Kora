using KX_Project.Repositories;
using System.IO;

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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IProductRepository,
MockProductRepository>();
builder.Services.AddScoped<ICategoryRepository,
MockCategoryRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");


app.Run();
