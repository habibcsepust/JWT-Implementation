using JwtAuth.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;

    public AuthController(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if ((request.Username == "admin" || request.Username == "user") && request.Password == "1234")
        {
            var tokens = _tokenService.GenerateTokens(request.Username, new List<string> { request.Username });
            return Ok(tokens);
        }
        return Unauthorized();
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] string refreshToken)
    {
        var newTokens = _tokenService.Refresh(refreshToken);
        if (newTokens == null)
            return Unauthorized("Invalid Refresh Token");

        return Ok(newTokens);
    }

    [HttpGet("secure")]
    [Authorize]
    public IActionResult Secure()
    {
        return Ok("You are authorized with Access Token!");
    }
}
