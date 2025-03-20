using FarmaciaLasFlores.Db;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult IniciarSesion(string NombreUsuario, string Password)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == NombreUsuario && u.Password == Password);

            if (usuario != null)
            {
                return RedirectToAction("Index", "Home");  // Redirige al Home si las credenciales son correctas
            }
            else
            {
                ViewBag.Error = "Credenciales incorrectas. Intente de nuevo.";
                return View("Index");
            }
        }
    }
}
    
