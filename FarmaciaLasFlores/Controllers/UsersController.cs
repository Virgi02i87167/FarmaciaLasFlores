using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FarmaciaLasFlores.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
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



        // Acción Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuariosViewModel model)
        {
            Console.WriteLine("Create action ejecutada");

            // Cargar la lista de roles para el formulario
            model.ListaRoles = await _context.Roles.ToListAsync();

            if (ModelState.IsValid)
            {
                try
                {
                    // Hash del password
                    model.NuevoUsuario.Password = HashPassword(model.NuevoUsuario.Password);
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
            // En tu controlador, antes de retornar la vista de creación
            ViewData["Roles"] = new SelectList(await _context.Roles.ToListAsync(), "Id", "NombreRoles");

            model.ListaUsuarios = await _context.Usuarios.ToListAsync();
            return View("Index", model);
        }

        // Acción Edit (POST)
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
        public async Task<IActionResult> Edit(int id, UsuariosViewModel viewModel)
        {
            // Cargar la lista de roles para el formulario de edición
            viewModel.ListaRoles = await _context.Roles.ToListAsync();

            if (id != viewModel.NuevoUsuario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Si el password no es nulo o vacío, hacer el hash
                    if (!string.IsNullOrEmpty(viewModel.NuevoUsuario.Password))
                    {
                        viewModel.NuevoUsuario.Password = HashPassword(viewModel.NuevoUsuario.Password);
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
    }
}
