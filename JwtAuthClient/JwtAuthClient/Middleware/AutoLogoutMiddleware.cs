namespace JwtAuthClient.Middleware
{
    public class AutoLogoutMiddleware
    {
        private readonly RequestDelegate _next;

        public AutoLogoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // এখানে HttpContext আসে automatically
        public async Task InvokeAsync(HttpContext context)
        {
            var tokenExpiry = context.Session.GetString("TokenExpiry");
            if (!string.IsNullOrEmpty(tokenExpiry) && DateTime.TryParse(tokenExpiry, out var expiryTime))
            {
                if (DateTime.UtcNow >= expiryTime)
                {
                    context.Session.Clear();
                    context.Response.Redirect("/Auth/Login?message=expired");
                    return;
                }
            }

            await _next(context);
        }
    }

}
