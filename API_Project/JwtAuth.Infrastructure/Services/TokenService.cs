using JwtAuth.Shared.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenService
{
    private readonly IConfiguration _config;
    private static List<RefreshToken> refreshTokens = new(); // Temporary store

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public AuthResponse GenerateTokens(string username, List<string> roles)
    {
        // Claims
        //var claims = new List<Claim>
        //{
        //    new Claim(JwtRegisteredClaimNames.Sub, username),
        //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //};

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "user") // বা "admin"
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        // Secret key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Access Token
        var token = new JwtSecurityToken(
            _config["JwtSettings:Issuer"],
            _config["JwtSettings:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(double.Parse(_config["JwtSettings:DurationInMinutes"])),
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Refresh Token
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            Username = username,
            ExpiryDate = DateTime.Now.AddDays(2) // 2 days valid
            //ExpiryDate = DateTime.Now.AddMinutes(2) // 2 minutes valid
        };
        refreshTokens.Add(refreshToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            Expiration = token.ValidTo
        };
    }

    public AuthResponse Refresh(string refreshToken)
    {
        var storedToken = refreshTokens.FirstOrDefault(t => t.Token == refreshToken);

        if (storedToken == null || storedToken.ExpiryDate < DateTime.Now)
            return null;

        // Generate new access token
        return GenerateTokens(storedToken.Username, new List<string> { "User" });
    }
}
