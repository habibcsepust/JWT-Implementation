var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();   // API call করার জন্য
builder.Services.AddSession();      // Token save করার জন্য

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();                   // Session ব্যবহার করা

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
