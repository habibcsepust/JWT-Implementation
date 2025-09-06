namespace JwtAuthClient.Services
{
    using JwtAuthClient.Models;
    using System.Net.Http.Json;

    public class TokenManager
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenManager(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> EnsureValidAccessTokenAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var expiryString = httpContext.Session.GetString("TokenExpiry");
            var refreshToken = httpContext.Session.GetString("RefreshToken");
            var accessToken = httpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(expiryString) || string.IsNullOrEmpty(accessToken))
                return false;

            var expiry = DateTime.Parse(expiryString);

            // Access Token is expired or not?
            if (DateTime.UtcNow >= expiry && !string.IsNullOrEmpty(refreshToken))
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsJsonAsync("http://localhost:5113/api/auth/refresh", refreshToken);

                if (response.IsSuccessStatusCode)
                {
                    var newTokens = await response.Content.ReadFromJsonAsync<TokenResponse>();

                    httpContext.Session.SetString("AccessToken", newTokens.AccessToken);
                    httpContext.Session.SetString("RefreshToken", newTokens.RefreshToken);
                    httpContext.Session.SetString("TokenExpiry", newTokens.Expiration.ToString());

                    return true; // Refresh success
                }
                else
                {
                    httpContext.Session.Clear(); // Refresh fail
                    return false;
                }
            }

            return true; // still valid
        }
    }

}
