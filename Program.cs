
using CampusFix.Data;
using CampusFix.Models;
using CampusFix.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<CampusFixDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<CampusFixDbContext>();

builder.Services.AddScoped<IMaintenanceRequestRepository, MaintenanceRequestRepository>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

async Task InitialiseDatabaseAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CampusFixDbContext>();

    // Apply any pending EF Core migrations before querying application tables.
    await db.Database.MigrateAsync();

    if (!await db.Categories.AnyAsync())
    {
        db.Categories.AddRange(
            new Category { Name = "Electrical", Description = "Power, lighting and sockets" },
            new Category { Name = "Plumbing", Description = "Water, drains and fixtures" },
            new Category { Name = "Cleaning", Description = "General cleaning and sanitation" },
            new Category { Name = "Security", Description = "Locks, alarms and access control" },
            new Category { Name = "Other", Description = "General campus facilities issues" });

        await db.SaveChangesAsync();
    }
}

async Task SeedRolesAndDemoMaintenanceUserAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Student", "Maintenance", "Administrator" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
                throw new InvalidOperationException($"Could not create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    // Optional demo account. Supply these as environment variables/user secrets;
    // do not commit real credentials to appsettings.json.
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var email = configuration["CampusFix:MaintenanceEmail"];
    var password = configuration["CampusFix:MaintenancePassword"];
    if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var maintenanceUser = await userManager.FindByEmailAsync(email);
        if (maintenanceUser == null)
        {
            maintenanceUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = "CampusFix Maintenance"
            };
            var createResult = await userManager.CreateAsync(maintenanceUser, password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException($"Could not create demo maintenance user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
        }

        if (!await userManager.IsInRoleAsync(maintenanceUser, "Maintenance"))
        {
            var roleResult = await userManager.AddToRoleAsync(maintenanceUser, "Maintenance");
            if (!roleResult.Succeeded)
                throw new InvalidOperationException("Could not assign the Maintenance role.");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

await InitialiseDatabaseAsync(app.Services);
await SeedRolesAndDemoMaintenanceUserAsync(app.Services);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

