using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyChat.Models;

namespace MyChat.Controllers;

public class MessageController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly MyChatContext _context;
    public MessageController(UserManager<User> userManager, MyChatContext context)
    {
        _context = context;
        _userManager = userManager;
    }
    public IActionResult Chat()
    {
        return View();
    }
    [HttpGet]
    public async Task<IActionResult> GetMessages()
    {
        var messages = await _context.Messages.Include(m => m.User).OrderByDescending(m => m.Created).Take(30).ToListAsync();
        return PartialView("_MessagesPartial", messages);
    }  
    public async Task<IActionResult> CreateAjax(string? username, string? body)
    {
        if (username != null && body != null)
        {
            User user = await _userManager.FindByNameAsync(username);
            if (user != null)
            {
                Message msg = new()
                {
                    UserId = user.Id,
                    Body = body
                };
                await _context.AddAsync(msg);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
        }
        return Json(new { success = false });
    }
}
