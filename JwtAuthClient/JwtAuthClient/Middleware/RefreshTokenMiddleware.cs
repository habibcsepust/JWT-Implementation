using JwtAuthClient.Services;

namespace JwtAuthClient.Middleware
{
    public class RefreshTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public RefreshTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, TokenManager tokenManager)
        {
            var token = context.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(token))
            {
                // Only for first time going to login page
                if (!context.Request.Path.Value.Contains("/Auth/Login"))
                {
                    context.Response.Redirect("/Auth/Login");
                    return;
                }
            }
            else
            {
                var success = await tokenManager.EnsureValidAccessTokenAsync();
                if (!success)
                {
                    context.Response.Redirect("/Auth/Login?message=expired");
                    return;
                }
            }
            await _next(context);
        }
    }
}