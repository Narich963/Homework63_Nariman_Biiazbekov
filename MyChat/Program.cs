using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MyChat.Models;
using MyChat.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization(opts => opts.ResourcesPath = "Resourses");
builder.Services.AddControllersWithViews()
    .AddViewLocalization();

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services
	.AddDbContext<MyChatContext>(opts => opts.UseNpgsql(connection))
	.AddIdentity<User, IdentityRole<int>>(opts =>
	{
		opts.Password.RequiredLength = 6;
		opts.Password.RequireUppercase = true;
		opts.Password.RequireLowercase = true;
		opts.Password.RequireDigit = true;
		opts.Password.RequireNonAlphanumeric = false;
	})
	.AddEntityFrameworkStores<MyChatContext>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
	var userManager = services.GetRequiredService<UserManager<User>>();
	var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
	await AdminInitializer.SeedAdminUser(roleManager, userManager);
}
catch (Exception ex)
{
	var logger = services.GetRequiredService<ILogger<Program>>();
	logger.LogError(ex, "An error has occured while seeding the database");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ru")
};

app.UseRequestLocalization(new RequestLocalizationOptions()
{
    DefaultRequestCulture = new RequestCulture("ru"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Message}/{action=Chat}/{id?}");

app.Run();
