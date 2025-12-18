using System.Text.Json;

namespace UserManagement.Web.Middleware
{
    public class PageAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public PageAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            // ===============================
            // 1️⃣ Allow static files
            // ===============================
            if (string.IsNullOrEmpty(path) ||
                path == "/" ||
                path == "/home" ||
                path == "/home/index" ||
                path.StartsWith("/assets") ||
                path.StartsWith("/css") ||
                path.StartsWith("/js"))
            {
                await _next(context);
                return;
            }

            // ===============================
            // 2️⃣ Allow login page & login POST
            // ===============================
            if (path == "/user/login")
            {
                await _next(context);
                return;
            }

            // ===============================
            // 3️⃣ Allow AJAX / API requests
            // ===============================
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                path.StartsWith("/api"))
            {
                await _next(context);
                return;
            }

            // ===============================
            // 4️⃣ If not logged in → redirect
            // ===============================
            if (!context.Session.Keys.Contains("UserId"))
            {
                context.Response.Redirect("/");
                return;
            }

            // ===============================
            // 5️⃣ Page authorization check
            // ===============================
            var pageUrlsJson = context.Session.GetString("PageUrls");

            if (string.IsNullOrEmpty(pageUrlsJson))
            {
                context.Response.Redirect("/");
                return;
            }

            // Extract only the page URL (before '|')
            var allowedPages = JsonSerializer
                .Deserialize<List<string>>(pageUrlsJson)
                ?.Select(p => p.Split('|')[0].ToLower())
                .ToList();

            if (allowedPages == null || !allowedPages.Contains(path))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("403 - Access Denied");
                return;
            }

            await _next(context);
        }
    }
}
