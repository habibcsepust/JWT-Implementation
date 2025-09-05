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
    public IActionResult Login(string? message)
    {
        if (message == "expired")
        {
            ViewBag.Message = "Your session has expired, please login again.";
        }
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
        var response = await client.PostAsJsonAsync("http://localhost:5113/api/auth/login", model);
        if (response.IsSuccessStatusCode)
        {
            var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
            // Session-এ token এবং expiry সময় রাখা
            HttpContext.Session.SetString("AccessToken", tokens.AccessToken);
            HttpContext.Session.SetString("RefreshToken", tokens.RefreshToken);
            //HttpContext.Session.SetString("TokenExpiry", DateTime.UtcNow.AddMinutes(15).ToString());
            HttpContext.Session.SetString("TokenExpiry", tokens.Expiration.ToString());

            return RedirectToAction("Index", "Home");
        }

        model.ErrorMessage = "Invalid username or password";
        return View(model);
    }


}
