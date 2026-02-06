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
                .OrderByDescending(x => x.LastLoginTime.HasValue)
                .ThenByDescending(x => x.LastLoginTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(users);
        }


        // ===== BLOCK =====
        [HttpPost]
        public async Task<IActionResult> Block(int[] userIds, bool selectAll)
        {
            var currentUserId = HttpContext.Session.GetInt32("user_id");

            List<ApplicationUser> users;

            if (selectAll)
            {
                users = await context.Users.ToListAsync();
            }
            else
            {
                if (userIds == null || userIds.Length == 0)
                    return RedirectToAction(nameof(Index));

                users = await context.Users
                    .Where(x => userIds.Contains(x.Id))
                    .ToListAsync();
            }

            var selfBlocked = false;

            foreach (var u in users)
            {
                u.Status = UserStatus.Blocked;

                if (u.Id == currentUserId)
                    selfBlocked = true;
            }

            await context.SaveChangesAsync();

            if (selfBlocked)
                HttpContext.Session.SetString("self_blocked", "1");

            return RedirectToAction(nameof(Index));
        }



        // ===== UNBLOCK =====
        [HttpPost]
        public async Task<IActionResult> Unblock(int[] userIds, bool selectAll)
        {
            List<ApplicationUser> users;

            if (selectAll)
            {
                users = await context.Users.ToListAsync();
            }
            else
            {
                if (userIds == null || userIds.Length == 0)
                    return RedirectToAction(nameof(Index));

                users = await context.Users
                    .Where(x => userIds.Contains(x.Id))
                    .ToListAsync();
            }

            foreach (var u in users)
            {
                if (u.Status == UserStatus.Blocked)
                    u.Status = UserStatus.Active;
            }

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ===== DELETE =====
        [HttpPost]
        public async Task<IActionResult> Delete(int[] userIds, bool selectAll)
        {
            List<ApplicationUser> users;

            if (selectAll)
            {
                users = await context.Users.ToListAsync();
            }
            else
            {
                if (userIds == null || userIds.Length == 0)
                    return RedirectToAction(nameof(Index));

                users = await context.Users
                    .Where(x => userIds.Contains(x.Id))
                    .ToListAsync();
            }

            context.Users.RemoveRange(users);

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // ===== DELETE UNVERIFIED =====
        [HttpPost]
        public async Task<IActionResult> DeleteUnverified()
        {
            var unverified = await context.Users
                .Where(x => x.Status == UserStatus.Unverified)
                .ToListAsync();

            context.Users.RemoveRange(unverified);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}