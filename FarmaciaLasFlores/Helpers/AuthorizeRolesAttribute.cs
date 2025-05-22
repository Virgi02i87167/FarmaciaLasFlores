namespace FarmaciaLasFlores.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AuthorizeRolesAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public AuthorizeRolesAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var rolUsuario = context.HttpContext.Session.GetString("RolUsuario");

        if (string.IsNullOrEmpty(rolUsuario) || !_roles.Contains(rolUsuario))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
        }
    }
}

