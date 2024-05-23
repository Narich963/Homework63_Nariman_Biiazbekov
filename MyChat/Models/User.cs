using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyChat.Models;

public class User : IdentityUser<int>
{
	[Required]
	public DateTime BirthDate { get; set; }
	[Required]
	public string Avatar { get; set; }

}
