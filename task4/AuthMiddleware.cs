using Microsoft.EntityFrameworkCore;
using task4.Data;
using task4.Models;

public class AuthMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, ApplicationDbContext db)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        if (path.StartsWith("/account/login") ||
            path.StartsWith("/account/register") ||
            path.StartsWith("/account/confirmemail") ||
            path.StartsWith("/css") ||
            path.StartsWith("/js") ||
            path.StartsWith("/lib") ||
            path.StartsWith("/images"))
        {
            await next(context);
            return;
        }

        var userId = context.Session.GetInt32("user_id");

        if (userId == null)
        {
            context.Response.Redirect("/Account/Login");
            return;
        }

        // important: проверяем флаг self-block ДО обращения к базе
        var selfBlocked = context.Session.GetString("self_blocked");

        if (selfBlocked == "1")
        {
            // nota bene: разрешаем один запрос пройти
            context.Session.Remove("self_blocked");
            await next(context);
            return;
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);

        // обычная проверка
        if (user == null || user.Status == UserStatus.Blocked)
        {
            context.Session.Clear();
            context.Response.Redirect("/Account/Login?msg=blocked");
            return;
        }

        await next(context);
    }
}