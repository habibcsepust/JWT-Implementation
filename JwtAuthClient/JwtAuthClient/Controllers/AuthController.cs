using JwtAuthClient.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
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
        // 1️⃣ Model validation
        if (!ModelState.IsValid)
        {
            model.ErrorMessage = "Please provide valid credentials.";
            return View(model);
        }

        // 2️⃣ Call API for login
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("http://localhost:5113/");

        var response = await client.PostAsJsonAsync("api/auth/login", new
        {
            Username = model.Username,
            Password = model.Password
        });

        if (!response.IsSuccessStatusCode)
        {
            model.ErrorMessage = "Invalid username or password.";
            return View(model);
        }

        // 3️⃣ Parse token response
        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
        if (tokens == null || string.IsNullOrEmpty(tokens.AccessToken))
        {
            model.ErrorMessage = "Failed to retrieve access token.";
            return View(model);
        }

        // 4️⃣ Save tokens in Session
        HttpContext.Session.SetString("AccessToken", tokens.AccessToken);
        HttpContext.Session.SetString("RefreshToken", tokens.RefreshToken ?? "");
        HttpContext.Session.SetString("TokenExpiry", tokens.Expiration.ToString());

        // 5️⃣ Extract claims from JWT
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokens.AccessToken);

        var claims = jwtToken.Claims.ToList();
        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        // 6️⃣ Sign-in with Cookie Authentication
        await HttpContext.SignInAsync("Cookies", principal);

        // 7️⃣ Role-based redirect
        var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        //if (role == "admin")
        //{
        //    return RedirectToAction("Privacy", "Home");
        //}
        //else if (role == "user")
        //{
        //    return RedirectToAction("Index", "Home");
        //}

        // 8️⃣ Default fallback
        return RedirectToAction("Index", "Home");
    }

}
