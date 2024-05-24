using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyChat.Models;

public class User : IdentityUser<int>
{
	[Required]
	public DateTime BirthDate { get; set; }
	[Required]
	public string Avatar { get; set; }

	public List<Message> Messages { get; set; }
    public User()
    {
        Messages = new List<Message>();
    }
}
