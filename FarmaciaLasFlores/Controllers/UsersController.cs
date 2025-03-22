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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuariosViewModel model)
        {
            Console.WriteLine("Create action ejecutada");
            if (ModelState.IsValid)
            {
                try
                {
                    model.NuevoUsuario.Password = BCrypt.Net.BCrypt.HashPassword(model.NuevoUsuario.Password);
                    _context.Usuarios.Add(model.NuevoUsuario);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Datos guardados exitosamente");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al guardar: {ex.Message}");
                    ModelState.AddModelError("", $"Ocurrió un error al guardar los datos: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("ModelState no válido");
            }

            // Si llegamos aquí, hubo un error de validación o una excepción
            // Rellenamos ListaUsuarios para que se muestre en la vista
            model.ListaUsuarios = await _context.Usuarios.ToListAsync();
            return View("Index", model);
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
