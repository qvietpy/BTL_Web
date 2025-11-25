using Microsoft.EntityFrameworkCore;
using QuanLyPhongTro.Data;
using QuanLyPhongTro.src.Services;
using Microsoft.AspNetCore.Authentication.Cookies;


var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register application services
builder.Services.AddScoped<RoomService>();

// Simple cookie auth (placeholder for real login)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Denied";
    });

builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });

// Add services to the container with custom Razor view locations to include the 'src' folder
builder.Services.AddControllersWithViews().AddRazorOptions(options =>
{
    // Views (non-area)
    options.ViewLocationFormats.Add("/src/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/src/Views/Shared/{0}.cshtml");

    // Areas
    options.AreaViewLocationFormats.Add("/src/Areas/{2}/Views/{1}/{0}.cshtml");
    options.AreaViewLocationFormats.Add("/src/Areas/{2}/Views/Shared/{0}.cshtml");
});

var app = builder.Build();

// Seed sample data for Renter page
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Enable area routing first
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default route to the existing Renter area Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Renter}/{controller=Home}/{action=Index}/{id?}");

app.Run();
