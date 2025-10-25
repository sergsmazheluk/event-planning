using EventPlanning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace EventPlanning.Api.Middleware
{
    public sealed class RequireCompletedProfileMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _whitelist = new[]
        {
        "/api/auth",
        "/api/profile",
        "/swagger",
        "/health"
    };

        public RequireCompletedProfileMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext ctx, UserManager<ApplicationUser> users)
        {
            if (ctx.User?.Identity?.IsAuthenticated == true)
            {
                // пропускаем whitelisted маршруты
                var path = ctx.Request.Path.Value ?? string.Empty;
                if (!_whitelist.Any(w => path.StartsWith(w, StringComparison.OrdinalIgnoreCase)))
                {
                    var user = await users.GetUserAsync(ctx.User);
                    if (user is not null && !user.ProfileCompleted)
                    {
                        ctx.Response.StatusCode = 428; // Precondition Required
                        await ctx.Response.WriteAsJsonAsync(new
                        {
                            error = "PROFILE_NOT_COMPLETED",
                            hint = "Call POST /api/profile/complete with your extra profile data."
                        });
                        return;
                    }
                }
            }

            await _next(ctx);
        }
    }

    public static class RequireCompletedProfileExtensions
    {
        public static IApplicationBuilder UseRequireCompletedProfile(this IApplicationBuilder app)
            => app.UseMiddleware<RequireCompletedProfileMiddleware>();
    }
}
