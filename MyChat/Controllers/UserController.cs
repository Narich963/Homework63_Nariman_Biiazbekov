using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyChat.Models;

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
    public async Task<IActionResult> Index()
    {
        return View(await _context.Users.ToListAsync());
    }

    // GET: User/Details/5
    public async Task<IActionResult> Details(string? name)
    {
        if (name == null)
        {
            return NotFound();
        }

        User user = await _userManager.FindByNameAsync(name);
        Console.WriteLine(User.IsInRole("admin"));
        if (user != null)
        {
            return View(user);
        }
        return NotFound();
    }

    // GET: User/Create
    [Authorize(Roles = "admin")]
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
    public async Task<IActionResult> Create(User user)
    {
        if (ModelState.IsValid)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    // GET: User/Edit/5
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
                oldUser.UserName = user.UserName;
                oldUser.Email = user.Email;
                oldUser.Avatar = user.Avatar;
                oldUser.BirthDate = user.BirthDate.ToUniversalTime();
                _context.Update(oldUser); 
                await _userManager.UpdateAsync(oldUser);
                await _context.SaveChangesAsync();
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

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}
