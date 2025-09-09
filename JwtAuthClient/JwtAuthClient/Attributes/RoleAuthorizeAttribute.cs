using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace JwtAuthClient.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // যদি authenticated না হয়
            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // যদি role match না হয়
            var role = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (!_roles.Contains(role))
            {
                context.Result = new RedirectToActionResult("Unauthorized", "Home", null);
            }
        }
    }
}
