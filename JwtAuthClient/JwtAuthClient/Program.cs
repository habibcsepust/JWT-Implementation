using JwtAuthClient.Middleware;
using JwtAuthClient.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ MVC / Razor View support
builder.Services.AddControllersWithViews();

// ✅ HttpClient factory
builder.Services.AddHttpClient();

// ✅ Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session lifetime
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ HttpContextAccessor for session in services
builder.Services.AddHttpContextAccessor();

// ✅ Custom service
builder.Services.AddScoped<TokenManager>();

// ✅ Authentication (Cookie ভিত্তিক)
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";             // যদি login করা না থাকে
        options.AccessDeniedPath = "/Home/Unauthorized"; // যদি role mismatch হয়
    });

// ✅ Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware pipeline
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // session আগে লাগাতে হবে
app.UseMiddleware<RefreshTokenMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// ✅ Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
