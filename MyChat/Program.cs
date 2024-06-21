using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using MyChat.Models;
using MyChat.Services;
using System.Globalization;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization(opts => opts.ResourcesPath = "Resourses");
builder.Services.AddResponseCompression(opts => opts.EnableForHttps = true);
builder.Services.Configure<BrotliCompressionProviderOptions>(opts =>
{
	opts.Level = CompressionLevel.Optimal;
});
builder.Services.Configure<GzipCompressionProviderOptions>(opts =>
{
	opts.Level = CompressionLevel.Optimal;
});
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
	.AddEntityFrameworkStores<MyChatContext>()
	.AddDefaultTokenProviders();
builder.Services.AddMemoryCache();

builder.Services.AddControllersWithViews(opts =>
{
	opts.CacheProfiles.Add("Caching", new CacheProfile()
	{
		Duration = 300
	});
	opts.CacheProfiles.Add("NoCaching", new CacheProfile()
	{
		Location = ResponseCacheLocation.None
	});
})
    .AddViewLocalization();

var app = builder.Build();

app.UseResponseCompression();

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
