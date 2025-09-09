using JwtAuthClient.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
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
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                                   .SelectMany(v => v.Errors)
                                   .Select(e => e.ErrorMessage)
                                   .ToList();
            // আপনি চাইলে errors TempData বা ViewBag এ পাঠাতে পারেন
            return View(model);
        }

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("http://localhost:5113/");

        var response = await client.PostAsJsonAsync("api/auth/login", new
        {
            Username = model.Username,
            Password = model.Password
        });

        if (!response.IsSuccessStatusCode)
        {
            model.ErrorMessage = "Invalid username or password";
            return View(model);
        }

        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();

        if (tokens == null || string.IsNullOrEmpty(tokens.AccessToken))
        {
            model.ErrorMessage = "Failed to retrieve access token.";
            return View(model);
        }

        // ✅ Token save to Session
        HttpContext.Session.SetString("AccessToken", tokens.AccessToken);
        HttpContext.Session.SetString("RefreshToken", tokens.RefreshToken);
        HttpContext.Session.SetString("TokenExpiry", tokens.Expiration.ToString());

        // Build User Authorize/Identity from JWT
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokens.AccessToken);

        // Claims identity
        var claims = jwtToken.Claims.ToList();
        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        // Cookie Authentication ব্যবহার করে login
        await HttpContext.SignInAsync("Cookies", principal);


        // Role based redirect
        var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        //if (role == "user")
        //{
        //    return RedirectToAction("Index", "Home");
        //}
        //else if (role == "admin")
        //{
        //    return RedirectToAction("Privacy", "Home");
        //}

        // Default fallback
        return RedirectToAction("Index", "Home");
    }

}
