using JwtAuthClient.Middleware;
using JwtAuthClient.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

//builder.Services.AddAuthentication("Bearer")
//    .AddJwtBearer("Bearer", options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
//            ValidAudience = builder.Configuration["JwtSettings:Audience"],
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
//        };
//    });

//builder.Services.AddAuthorization();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

// 👉 অবশ্যই session আগে enable করতে হবে
app.UseSession();

// 👉 RefreshTokenMiddleware auto session check করবে
app.UseMiddleware<RefreshTokenMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// 👉 Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Login successful হলে Home এ যাবে

app.Run();
