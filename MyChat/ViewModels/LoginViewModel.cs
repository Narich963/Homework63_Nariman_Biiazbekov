using System.ComponentModel.DataAnnotations;

namespace MyChat.ViewModels;

public class LoginViewModel
{
	[Required(ErrorMessage = "Login field is empty")]
	public string Login { get; set; }

	[Required(ErrorMessage = "Password field is empty")]
	[DataType(DataType.Password)]
	public string Password { get; set; }
}
