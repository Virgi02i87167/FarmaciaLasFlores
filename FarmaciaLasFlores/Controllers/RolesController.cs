using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaLasFlores.Controllers
{
    [Authorize(Roles = "Admin")] // Protege todo el controlador para solo admins
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Muestra lista de roles y formulario de creación
        public async Task<IActionResult> Index()
        {
            var model = new RolesViewModel
            {
                NuevoRol = new Roles(),
                ListaRoles = await _context.Roles.ToListAsync()
            };
            return View(model);
        }

        // POST: Crea un nuevo rol
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RolesViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validar si el rol ya existe
                    if (await _context.Roles.AnyAsync(r => r.NombreRoles == model.NuevoRol.NombreRoles))
                    {
                        ModelState.AddModelError("NuevoRol.Nombre", "Este nombre de rol ya existe");
                        model.ListaRoles = await _context.Roles.ToListAsync();
                        return View("Index", model);
                    }

                    _context.Roles.Add(model.NuevoRol);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                }
            }

            model.ListaRoles = await _context.Roles.ToListAsync();
            return View("Index", model);
        }

        // GET: Muestra formulario para editar rol
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                return NotFound();
            }

            return View(rol);
        }

        // POST: Actualiza un rol existente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion")] Roles rol)
        {
            if (id != rol.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validar nombre único (excluyendo el actual)
                    if (await _context.Roles.AnyAsync(r => r.NombreRoles == rol.NombreRoles && r.Id != rol.Id))
                    {
                        ModelState.AddModelError("Nombre", "Este nombre de rol ya está en uso");
                        return View(rol);
                    }

                    _context.Update(rol);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RolExists(rol.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(rol);
        }

        // POST: Elimina un rol
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                return NotFound();
            }

            try
            {
                _context.Roles.Remove(rol);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "No se puede eliminar el rol porque está en uso.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        private bool RolExists(int id)
        {
            return _context.Roles.Any(e => e.Id == id);
        }
    }
}