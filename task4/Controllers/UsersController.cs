using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task4.Data;
using task4.Models;

namespace task4.Controllers
{
    public class UsersController(ApplicationDbContext context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var users = await context.Users
                .OrderByDescending(x => x.LastLoginTime)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string action, int[] userIds)
        {
            if (action == "delete_unverified")
            {
                var unverified = await context.Users
                    .Where(x => x.Status == UserStatus.Unverified)
                    .ToListAsync();

                context.Users.RemoveRange(unverified);
                await context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            if (userIds == null || userIds.Length == 0)
                return RedirectToAction(nameof(Index));

            var users = await context.Users
                .Where(x => userIds.Contains(x.Id))
                .ToListAsync();

            switch (action)
            {
                case "block":
                    foreach (var u in users)
                        u.Status = UserStatus.Blocked;
                    break;

                case "unblock":
                    foreach (var u in users)
                    {
                        if (u.Status == UserStatus.Blocked)
                            u.Status = UserStatus.Active;
                    }
                    break;

                case "delete":
                    context.Users.RemoveRange(users);
                    break;
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }

}
