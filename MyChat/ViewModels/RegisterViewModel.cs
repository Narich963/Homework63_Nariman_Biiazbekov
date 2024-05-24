using System.ComponentModel.DataAnnotations;

namespace MyChat.ViewModels;

public class RegisterViewModel
{
	[Required(ErrorMessage = "Username field is empty")]
	public string Username { get; set; }

	[Required(ErrorMessage = "Email field is empty")]
	public string Email { get; set; }

	[Required(ErrorMessage = "Birth date field is empty")]
	public DateTime BirthDate { get; set; }

	[Required(ErrorMessage = "Avatar field is empty")]
	public string Avatar { get; set; }

	[Required(ErrorMessage = "Password field is empty")]
	[DataType(DataType.Password)]
	public string Password { get; set; }

	[DataType(DataType.Password)]
	[Compare("Password", ErrorMessage = "Passwords are not equal")]
	public string ConfirmPassword { get; set; }
}
