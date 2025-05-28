using FarmaciaLasFlores.Db;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaLasFlores.Controllers
{
    public class LoginController : Controller
    {
  
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IniciarSesionAsync(string NombreUsuario, string Password)
        {
            //var hashedPassword = HashPassword(Password);

            //var usuario = _context.Usuarios
            //.Include(u => u.Rol)  // <-- incluir el rol
            //.FirstOrDefault(u => u.NombreUsuario == NombreUsuario && u.Password == hashedPassword);
            var usuario = _context.Usuarios
          .Include(u => u.Rol)  // <-- incluir el rol
          .FirstOrDefault(u => u.NombreUsuario == NombreUsuario && u.Password == Password);

            if (usuario != null)
            {
                if (usuario.Estado == true)
                {
                    HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                    HttpContext.Session.SetString("RolUsuario", usuario.Rol.NombreRoles);  // <-- guardar rol en sesión

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario.Rol.NombreRoles) // <-- Agregar claim de rol
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Error = "Este usuario no existe.";
                    return View("Index");
                }
            }
            else
            {
                ViewBag.Error = "Credenciales incorrectas. Intente de nuevo.";
                return View("Index");
            }
        }


        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}
    
