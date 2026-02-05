using Microsoft.EntityFrameworkCore;
using task4.Data;
using task4.Models;

namespace task4
{
    public class AuthMiddleware(RequestDelegate nextRequest)
    {
        public async Task Invoke(HttpContext context, ApplicationDbContext db)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            if (path.Contains("/account/login") ||
                path.Contains("/account/register") ||
                path.Contains("/account/logout") ||
                path.Contains("/css") ||
                path.Contains("/js") ||
                path.Contains("/lib"))
            {
                await nextRequest(context);
                return;
            }

            var userId = context.Session.GetInt32("user_id");

            // если не залогинен
            if (userId == null)
            {
                context.Response.Redirect("/Account/Login");
                return;
            }

            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);

            // nota bene: если удалён или заблокирован
            if (user == null || user.Status == UserStatus.Blocked)
            {
                context.Session.Clear();
                context.Response.Redirect("/Account/Login");
                return;
            }

            await nextRequest(context);
        }
    }
}
