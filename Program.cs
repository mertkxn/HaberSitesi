using System.Threading.RateLimiting;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SonGun.Data;
using SonGun.Models;
using SonGun.Services;

var candidatePaths = new[]
{
    Path.Combine(Directory.GetCurrentDirectory(), ".env"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"),
    Path.Combine(AppContext.BaseDirectory, ".env"),
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env"),
};

// Searches candidate directories for the .env configuration file
foreach (var path in candidatePaths)
{
    // Loads environment variables from the first matching .env file and exits loop
    if (File.Exists(path))
    {
        Env.Load(path);
        break;
    }
}

var builder = WebApplication.CreateBuilder(args);

// Retrieves configuration with priority: environment variables (.env) -> appsettings.json -> fallback value
string GetEnvOrConfig(string key, string fallback)
{
    var val = Environment.GetEnvironmentVariable(key);
    // Returns trimmed environment variable if defined and non-empty
    if (!string.IsNullOrWhiteSpace(val)) return val.Trim();
    val = builder.Configuration[key];
    // Returns trimmed configuration setting from appsettings.json if present
    if (!string.IsNullOrWhiteSpace(val)) return val.Trim();
    return fallback;
}

// Binds administrator and editor credentials to strongly-typed AuthSettings
builder.Services.Configure<AuthSettings>(options =>
{
    options.AdminUsername = GetEnvOrConfig("ADMIN_USERNAME", "admin");
    options.AdminPassword = GetEnvOrConfig("ADMIN_PASSWORD", "12345");
    options.EditorUsername = GetEnvOrConfig("EDITOR_USERNAME", "editor");
    options.EditorPassword = GetEnvOrConfig("EDITOR_PASSWORD", "12345");
});

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=songun.db"));

// Configures cookie-based authentication scheme with security policies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "SonGun.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Protects the login endpoint against brute-force attacks by limiting requests to 30 per minute
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("login-limiter", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

var app = builder.Build();

// Creates a scoped service container to execute startup database initialization
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Applies pending EF Core migrations automatically to ensure schema is up to date
    dbContext.Database.Migrate();
    DbSeeder.SeedAsync(dbContext).GetAwaiter().GetResult();
    SqlExporter.ExportToSql(dbContext, Path.Combine(app.Environment.ContentRootPath, "database.sql"));

    // Exports the database content into a static HTML site if CLI export flag is provided
    if (args.Contains("--export") || args.Contains("export"))
    {
        var outputDir = args.FirstOrDefault(a => a != "--export" && a != "export" && !a.StartsWith("-")) ?? "docs";
        StaticSiteGenerator.Generate(dbContext, outputDir);
        Console.WriteLine($"[SUCCESS] Static site generated at: '{outputDir}'");
        return;
    }
}

// Enables production-grade error handler and HSTS when not in development environment
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
