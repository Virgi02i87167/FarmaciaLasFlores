using FarmaciaLasFlores.Db;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        //Si quieren crear una vista de un controlador facilmente, solo denle click derecho a Index y seleccionen "Add View"
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IniciarSesionAsync(string NombreUsuario, string Password)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == NombreUsuario && u.Password == Password);

            if (usuario != null)
            {
                // Crear los claims para el usuario autenticado
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                    // Puedes agregar más claims si es necesario
                };

                // Crear el identity de la cookie
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Establecer la autenticación
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                return RedirectToAction("Index", "Home");  // Redirige al Home si las credenciales son correctas
            }
            else
            {
                ViewBag.Error = "Credenciales incorrectas. Intente de nuevo.";
                return View("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}
    
