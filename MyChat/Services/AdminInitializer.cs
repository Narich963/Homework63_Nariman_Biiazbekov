using Microsoft.AspNetCore.Identity;
using MyChat.Models;

namespace MyChat.Services;

public class AdminInitializer
{
	public static async Task SeedAdminUser(RoleManager<IdentityRole<int>> _roleManager, UserManager<User> _userManager)
	{
		string adminEmail = "admin";
		string adminPassword = "Admin1";

		var roles = new[] { "admin", "user" };

		foreach (var role in roles)
		{
			if (await _roleManager.FindByNameAsync(role) is null)
			{
				await _roleManager.CreateAsync(new IdentityRole<int>(role));
			}
		}
		if (await _userManager.FindByNameAsync(adminEmail) == null)
		{
			User admin = new User()
			{
				Email = adminEmail,
				UserName = adminEmail,
				Avatar = "https://media.licdn.com/dms/image/C4E03AQGO448nAOrvfw/profile-displayphoto-shrink_400_400/0/1516929476300?e=2147483647&v=beta&t=i9xTbCh2nx3upQEx53PPtGP28Da2T7i_AJOTsqQRliE"
			};
			IdentityResult result = await _userManager.CreateAsync(admin, adminPassword);
			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(admin, "admin");
			}
		}
	}
}
