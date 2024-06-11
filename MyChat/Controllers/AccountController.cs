using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyChat.Models;
using MyChat.ViewModels;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace MyChat.Controllers;

public class AccountController : Controller
{
	private readonly UserManager<User> _userManager;
	private readonly SignInManager<User> _signInManager;
    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    [HttpGet]
    [ResponseCache(CacheProfileName = "Caching")]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ResponseCache(CacheProfileName = "Caching")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            User user = await _userManager.FindByEmailAsync(model.Login) ?? await _userManager.FindByNameAsync(model.Login);
            if (user != null)
            {
                if (user.LockoutEnabled == true && user.LockoutEnd > DateTime.UtcNow)
                {
                    ModelState.AddModelError("", "Ваш аккаунт заблокирован");
                }
                SignInResult result = await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    isPersistent: false,
                    lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                return View(model);
            }
			ModelState.AddModelError("", "Неверный логин или пароль");
		}
        return View(model);
    }

    [HttpGet]
    [ResponseCache(CacheProfileName = "Caching")]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ResponseCache(CacheProfileName = "Caching")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            if ((DateTime.UtcNow - model.BirthDate).TotalDays < 18 * 365)
            {
                ModelState.AddModelError("", "Недопустимый возраст регистрации");
                return View(model);
            }
            if (_userManager.Users.Any(u => u.Email == model.Email || u.UserName == model.Username))
            {
                ModelState.AddModelError("", "Пользователь с тамим именем или эл. почтой уже существует");
                return View(model);
            }
            User? user = new()
            {
                Email = model.Email,
                UserName = model.Username,
                Avatar = model.Avatar,
                BirthDate = model.BirthDate.ToUniversalTime()
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "user");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        return View(model);
    }
    [ResponseCache(CacheProfileName = "Caching")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
