using System.ComponentModel.DataAnnotations;

namespace JwtAuthClient.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        public string? ErrorMessage { get; set; } // nullable

    }
}
