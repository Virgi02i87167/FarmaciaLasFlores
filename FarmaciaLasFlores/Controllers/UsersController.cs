using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Models;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            var viewModel = new UsuariosViewModel
            {
                NuevoUsuario = usuario,
                ListaUsuarios = await _context.Usuarios.ToListAsync()
            };
            return View(viewModel);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuariosViewModel viewModel)
        {
            if (id != viewModel.NuevoUsuario.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrEmpty(viewModel.NuevoUsuario.Password))
                    {
                        viewModel.NuevoUsuario.Password = BCrypt.Net.BCrypt.HashPassword(viewModel.NuevoUsuario.Password);
                    }
                    _context.Update(viewModel.NuevoUsuario);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Usuario actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(viewModel.NuevoUsuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(viewModel);
        }


        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
