using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuth.Shared.DTOs
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
