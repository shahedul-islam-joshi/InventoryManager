using InventoryManager.Data;
using InventoryManager.Models;
using InventoryManager.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. DATABASE CONFIGURATION (Enhanced Parser)
// ==========================================
// We check for "DefaultConnection" (Local) or "DATABASE_URL" (Render)
// 1. Prioritize Render/Production environment variable
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string not found.");
}

// Convert postgres:// URI to the Key=Value format Npgsql requires
if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');

    // Fix for the 'Port -1' error: If port is missing in URI, use default 5432
    var port = uri.Port <= 0 ? 5432 : uri.Port;

    connectionString = $"Host={uri.Host};" +
                       $"Port={port};" +
                       $"Database={uri.AbsolutePath.TrimStart('/')};" +
                       $"Username={userInfo[0]};" +
                       $"Password={userInfo[1]};" +
                       $"SslMode=Require;" +
                       $"Trust Server Certificate=true;";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ==========================================
// 2. IDENTITY SETUP
// ==========================================
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register AccessService so it can be injected into controllers.
// Scoped lifetime means one instance per HTTP request — appropriate for EF Core usage.
builder.Services.AddScoped<InventoryManager.Services.Interfaces.IAccessService,
                            InventoryManager.Services.AccessService>();

// Register DiscussionService for discussion persistence
builder.Services.AddScoped<InventoryManager.Services.Interfaces.IDiscussionService,
                            InventoryManager.Services.DiscussionService>();

// Register SignalR — built into ASP.NET Core, no extra NuGet package required
builder.Services.AddSignalR();

// Register SearchService for full-text search via PostgreSQL FTS
builder.Services.AddScoped<InventoryManager.Services.Interfaces.ISearchService,
                            InventoryManager.Services.SearchService>();



var app = builder.Build();

// ==========================================
// 3. MIDDLEWARE PIPELINE
// ==========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
// Order is critical: Custom middleware must be after Auth and before AuthZ
app.UseMiddleware<InventoryManager.Middleware.BlockedUserMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Map the SignalR hub endpoint — clients connect to /discussionHub
app.MapHub<InventoryManager.Hubs.DiscussionHub>("/discussionHub");


// ==========================================
// 4. AUTO-MIGRATE & SEED DATA
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // This line creates the tables in your Render database automatically
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed Admin Role
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Seed Default Admin User
        var adminEmail = "admin@admin.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsBlocked = false
            };

            var result = await userManager.CreateAsync(user, "admin123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during migration or seeding.");
    }
}

app.Run();