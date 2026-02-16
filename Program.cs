using InventoryManager.Data;
using InventoryManager.Models;
using InventoryManager.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. DATABASE CONFIGURATION (PostgreSQL)
// ==========================================
// This pulls from User Secrets locally or Environment Variables on Render
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ==========================================
// 2. IDENTITY & SECURITY
// ==========================================
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// ==========================================
// 3. MVC & RAZOR PAGES
// ==========================================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// ==========================================
// 4. MIDDLEWARE PIPELINE
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
// Custom Middleware - Placed after Auth to identify the user
app.UseMiddleware<InventoryManager.Middleware.BlockedUserMiddleware>();
app.UseAuthorization();

// ==========================================
// 5. ROUTING
// ==========================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ==========================================
// 6. AUTO-MIGRATE & SEED DATA (Fixes Render)
// ==========================================
// Using a scope to ensure services are disposed of properly
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // CRITICAL: This builds your tables on Render automatically
        if (context.Database.GetPendingMigrations().Any())
        {
            await context.Database.MigrateAsync();
        }

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Create Admin role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Create Default Admin User
        var adminEmail = "admin@admin.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
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