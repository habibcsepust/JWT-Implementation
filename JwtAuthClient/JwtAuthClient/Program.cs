using JwtAuthClient.Middleware;
using JwtAuthClient.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC / Razor View support
builder.Services.AddControllersWithViews();

// HttpClient factory
builder.Services.AddHttpClient();

// Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session lifetime
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// HttpContextAccessor for session in services
builder.Services.AddHttpContextAccessor();

// Custom service
builder.Services.AddScoped<TokenManager>();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

// 👉 অবশ্যই session আগে enable করতে হবে
app.UseSession();

// 👉 RefreshTokenMiddleware auto session check করবে
app.UseMiddleware<RefreshTokenMiddleware>();

app.UseAuthorization();

// 👉 Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Login successful হলে Home এ যাবে

app.Run();
