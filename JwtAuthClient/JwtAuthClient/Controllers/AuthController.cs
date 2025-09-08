using JwtAuthClient.Models;
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
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                     .Select(e => e.ErrorMessage)
                                     .ToList();
            return View(model);
        }


        var client = _httpClientFactory.CreateClient();
        
        //var response = await client.PostAsJsonAsync("http://localhost:5113/api/auth/login", model);
        string baseUrl = _config["ApiSettings:BaseUrl"];
        var response = await client.PostAsJsonAsync($"{baseUrl}auth/login", model);

        if (response.IsSuccessStatusCode)
        {
            var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
            // Session-এ token এবং expiry সময় রাখা
            HttpContext.Session.SetString("AccessToken", tokens.AccessToken);
            HttpContext.Session.SetString("RefreshToken", tokens.RefreshToken);
            //HttpContext.Session.SetString("TokenExpiry", DateTime.UtcNow.AddMinutes(15).ToString());
            HttpContext.Session.SetString("TokenExpiry", tokens.Expiration.ToString());

            // Get Role from Token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokens.AccessToken);

            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Redirect as per role
            if (role == "user")
            {
                return RedirectToAction("Privacy", "Home");
            }
            else if (role == "admin")
            {
                return RedirectToAction("Index", "Home");
            }

            // Default redirect
            return RedirectToAction("Index", "Home");
        }

        model.ErrorMessage = "Invalid username or password";
        return View(model);
    }


}
