using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaLasFlores.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            return View("../Views/Users/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuariosViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.NuevoUsuario.Password = BCrypt.Net.BCrypt.HashPassword(model.NuevoUsuario.Password); // Encriptar contraseña

                _context.Usuarios.Add(model.NuevoUsuario);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(model);
        }


        public async Task<IActionResult> Index()
        {
            var modelo = new UsuariosViewModel
            {
                NuevoUsuario = new Usuarios(),
                ListaUsuarios = await _context.Usuarios.ToListAsync()
            };

            return View(modelo);
        }

    }
}
