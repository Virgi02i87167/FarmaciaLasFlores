using System.Linq.Expressions;
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

        public async Task<IActionResult> Index()
        {
            var modelo = new UsuariosViewModel
            {
                NuevoUsuario = new Usuarios(),
                ListaUsuarios = await _context.Usuarios.ToListAsync()
            };
            return View(modelo);
        }

        //GET: Users/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Usuario = await _context.Usuarios.FindAsync(id);
            if (Usuario == null)
            {
                return NotFound();
            }

            return View(Usuario);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreUsuario,Password")] Usuarios usuarios)
        {
            if (id != usuarios.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrEmpty(usuarios.Password))
                    {
                        usuarios.Password = BCrypt.Net.BCrypt.HashPassword(usuarios.Password);
                    }

                    _context.Update(usuarios);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Usuario Actualizado Exitosamente.";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuarios.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ocurrio un error al actualizar el usuario: {ex.Message}");
                }
            }

            return View(usuarios);
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}