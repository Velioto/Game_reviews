using Game_reviews.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity + Roles
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // set true later when you have email confirmation
})
.AddRoles<IdentityRole>() // IMPORTANT
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // IMPORTANT
app.UseAuthorization();

// Seed roles + admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAndAdminAsync(services);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Games}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

static async Task SeedRolesAndAdminAsync(IServiceProvider services)
{
    const string ADMIN_ROLE = "Admin";
    const string USER_ROLE = "User";

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    if (!await roleManager.RoleExistsAsync(ADMIN_ROLE))
        await roleManager.CreateAsync(new IdentityRole(ADMIN_ROLE));

    if (!await roleManager.RoleExistsAsync(USER_ROLE))
        await roleManager.CreateAsync(new IdentityRole(USER_ROLE));

    // Default admin user
    var adminEmail = "admin@game.local";
    var adminPassword = "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createResult.Succeeded)
            throw new Exception(string.Join("\n", createResult.Errors.Select(e => e.Description)));
    }

    if (!await userManager.IsInRoleAsync(adminUser, ADMIN_ROLE))
        await userManager.AddToRoleAsync(adminUser, ADMIN_ROLE);
}