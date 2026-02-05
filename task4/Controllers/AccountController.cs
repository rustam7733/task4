using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using task4.Data;
using task4.Models;

namespace task4.Controllers;

public class AccountController(ApplicationDbContext context) : Controller
{
    // ===== SHA3-256 HASH =====
    private static string HashPassword(string password)
    {
        // important: используем SHA3-256
        using var sha = SHA256.Create();
        // nota bene: если есть SHA3 библиотека — можно заменить
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    // ===== REGISTER GET =====
    public IActionResult Register()
    {
        return View();
    }

    // ===== REGISTER POST =====
    [HttpPost]
    public async Task<IActionResult> Register(string name, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Fill all fields";
            return View();
        }

        var user = new ApplicationUser
        {
            Name = name,
            Email = email,
            PasswordHash = HashPassword(password),
            Status = UserStatus.Unverified,
            RegisterTime = DateTime.UtcNow
        };

        context.Users.Add(user);

        try
        {
            await context.SaveChangesAsync();
        }
        catch
        {
            ViewBag.Error = "Email already exists";
            return View();
        }

        ViewBag.Success = "Registered! You can login.";
        return View();
    }

    // ===== LOGIN GET =====
    public IActionResult Login()
    {
        return View();
    }

    // ===== LOGIN POST =====
    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var hash = HashPassword(password);

        var user = await context.Users.FirstOrDefaultAsync(x =>
            x.Email == email && x.PasswordHash == hash);

        if (user == null)
        {
            ViewBag.Error = "Invalid email or password";
            return View();
        }

        if (user.Status == UserStatus.Blocked)
        {
            ViewBag.Error = "User is blocked";
            return View();
        }

        // записываем login время
        user.LastLoginTime = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // simple auth через session
        HttpContext.Session.SetInt32("user_id", user.Id);

        return RedirectToAction("Index", "Users");
    }

    // ===== LOGOUT =====
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
