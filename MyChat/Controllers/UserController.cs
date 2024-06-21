using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyChat.Models;
using MyChat.Services;
using MyChat.ViewModels;

namespace MyChat.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly MyChatContext _context;
    private readonly UserManager<User> _userManager; 

    public UserController(MyChatContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: User
    [Authorize(Roles = "admin")]
    [ResponseCache(CacheProfileName = "NoCaching")]
    public async Task<IActionResult> Index()
    {
        return View(await _context.Users.ToListAsync());
    }

    // GET: User/Details/5
    [ResponseCache(CacheProfileName = "Caching")]
    public async Task<IActionResult> Details(string? name)
    {
        if (name == null)
        {
            return NotFound();
        }

        User user = await _context.Users
            .Include(u => u.Messages)
            .FirstOrDefaultAsync(u => u.UserName == name);
        if (user != null)
        {
            return View(user);
        }
        return NotFound();
    }

    // GET: User/Create
    [Authorize(Roles = "admin")]
    [ResponseCache(CacheProfileName = "Caching")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: User/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admin")]
    [ResponseCache(CacheProfileName = "Caching")]
    public async Task<IActionResult> Create(RegisterViewModel model)
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
                return RedirectToAction("Index", "User");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        return View(model);
        
    }

    // GET: User/Edit/5
    [ResponseCache(CacheProfileName = "Caching")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }
        if (user.UserName == User.Identity.Name || User.IsInRole("admin"))
        {
            return View(user);
        }
        return NotFound();
    }

    // POST: User/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ResponseCache(CacheProfileName = "Caching")]
    public async Task<IActionResult> Edit(int id, User user)
    {
        if (id != user.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                User oldUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                string text = $"""
                    Вы изменили свой профиль:
                                      До - после
                    Имя пользователя: {oldUser.UserName} - {user.UserName},
                    Почта:            {oldUser.Email} - {user.Email},
                    Фото профиля:     {oldUser.Avatar} - {user.Avatar},
                    Дата рождения:    {oldUser.BirthDate} - {user.BirthDate}
                    """;
                oldUser.UserName = user.UserName;
                oldUser.Email = user.Email;
                oldUser.Avatar = user.Avatar;
                oldUser.BirthDate = user.BirthDate.ToUniversalTime();
                _context.Update(oldUser); 
                await _userManager.UpdateAsync(oldUser);
                await _context.SaveChangesAsync();
                EmailService service = new();
                service.SendEmail(oldUser.Email, "Редактирование профиля", text);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    // GET: User/Delete/5
    [Authorize(Roles = "admin")]
    [ResponseCache(CacheProfileName = "Caching")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // POST: User/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admin")]
    [ResponseCache(CacheProfileName = "Caching")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = "admin")]
    [ResponseCache(CacheProfileName = "NoCaching")]
    public async Task<IActionResult> Block(int? id)
    {
        if (id != null)
        {
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                var lockUserTask = await _userManager.SetLockoutEnabledAsync(user, true);

                var lockDateTask = await _userManager.SetLockoutEndDateAsync(user, DateTime.MaxValue.ToUniversalTime());

                if (lockDateTask.Succeeded && lockUserTask.Succeeded)
                {
                    Console.WriteLine(user.LockoutEnabled + " " + user.LockoutEnd);
                    return RedirectToAction("Index");
                }
            }
        }
        return NotFound();
    }
    [Authorize(Roles = "admin")]
    [ResponseCache(CacheProfileName = "NoCaching")]
    public async Task<IActionResult> UnBlock(int? id)
    {
        if (id != null)
        {
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                var lockDateTask = await _userManager.SetLockoutEndDateAsync(user, (DateTime.UtcNow - TimeSpan.FromDays(1)).ToUniversalTime());
                var lockUserTask = await _userManager.SetLockoutEnabledAsync(user, false);

                if (lockDateTask.Succeeded && lockUserTask.Succeeded)
                {
                    return RedirectToAction("Index");
                }
            }
        }
        return NotFound();
    }
    [Authorize]
    public async Task<IActionResult> GetData(int id)
    {
        User user = await _context.Users.Include(u => u.Messages).FirstOrDefaultAsync(u => u.Id == id);
        if (user != null)
        {
            string text = $"""
                Имя пользователя - {user.UserName},
                Email = {user.Email},
                Дата рождения - {user.BirthDate},
                Фото профиля - {user.Avatar},
                Сообщения:
                """;
            foreach (var m in user.Messages)
            {
                text += "\n" + "\t" + m.Body + " " + m.Created.ToShortTimeString();
            }
            EmailService service = new();
            service.SendEmail(user.Email, "Запрос на получение данных", text);
            return RedirectToAction("Details", new {name = user.UserName});
        }
        return NotFound();
    }
    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}
