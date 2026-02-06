using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task4.Data;
using task4.Models;

namespace task4.Controllers
{
    public class UsersController(ApplicationDbContext context) : Controller
    {
        // ===== LIST =====
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 20;

            var totalUsers = await context.Users.CountAsync();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var users = await context.Users
                .OrderByDescending(x => x.LastLoginTime)
                .ThenByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(users);
        }


        // ===== BLOCK =====
        [HttpPost]
        public async Task<IActionResult> Block(int[] userIds, int page)
        {
            if (userIds == null || userIds.Length == 0)
                return RedirectToAction(nameof(Index), new { page });

            var currentUserId = HttpContext.Session.GetInt32("user_id");

            // important: если пользователь блокирует сам себя
            if (currentUserId != null && userIds.Contains(currentUserId.Value))
            {
                HttpContext.Session.SetString("self_blocked", "1");
            }

            await context.Users
                .Where(x => userIds.Contains(x.Id))
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(u => u.Status, UserStatus.Blocked));

            return RedirectToAction(nameof(Index), new { page });
        }


        // ===== UNBLOCK =====
        [HttpPost]
        public async Task<IActionResult> Unblock(int[] userIds, int page)
        {
            if (userIds == null || userIds.Length == 0)
                return RedirectToAction(nameof(Index), new { page });

            await context.Users
                .Where(x => userIds.Contains(x.Id))
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(u => u.Status, UserStatus.Active));

            return RedirectToAction(nameof(Index), new { page });
        }


        // ===== DELETE =====
        [HttpPost]
        public async Task<IActionResult> Delete(int[] userIds, int page)
        {
            if (userIds == null || userIds.Length == 0)
                return RedirectToAction(nameof(Index), new { page });

            await context.Users
                .Where(x => userIds.Contains(x.Id))
                .ExecuteDeleteAsync();

            return RedirectToAction(nameof(Index), new { page });
        }


        // ===== DELETE UNVERIFIED =====
        [HttpPost]
        public async Task<IActionResult> DeleteUnverified()
        {
            // important: это глобальная операция по всей базе,
            // не зависит от чекбоксов

            var unverifiedUsers = await context.Users
                .Where(x => x.Status == UserStatus.Unverified)
                .ToListAsync();

            context.Users.RemoveRange(unverifiedUsers);
            await context.SaveChangesAsync();

            TempData["msg"] = "All unverified users deleted";
            return RedirectToAction(nameof(Index));
        }
    }
}