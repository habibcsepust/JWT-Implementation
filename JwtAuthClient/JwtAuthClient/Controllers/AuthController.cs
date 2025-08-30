using JwtAuthClient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                     .Select(e => e.ErrorMessage)
                                     .ToList();
            return View(model);
        }
            

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("http://localhost:5113/"); 

        var response = await client.PostAsJsonAsync("api/auth/login", new
        {
            Username = model.Username,
            Password = model.Password
        });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            string token = result.GetProperty("accessToken").GetString();
            HttpContext.Session.SetString("JwtToken", token);


            return RedirectToAction("Index", "Home"); // Secure Page / Dashboard
        }

        model.ErrorMessage = "Invalid username or password";
        return View(model);
    }
}
