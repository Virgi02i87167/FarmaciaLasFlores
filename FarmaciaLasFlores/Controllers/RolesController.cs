using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Helpers;
using FarmaciaLasFlores.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaLasFlores.Controllers
{
    [AuthorizeRoles("Administrador")]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Acción GET para mostrar el formulario de creación y los roles existentes
        public async Task<IActionResult> Index()
        {
            var model = new RolesViewModel
            {
                NuevoRol = new Roles(),
                ListaRoles = await _context.Roles.ToListAsync()  // Carga los roles existentes
            };
            return View(model);  // Retorna la vista Index con el modelo
        }

        // Acción POST para crear un nuevo rol
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RolesViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Agregar el nuevo rol a la base de datos
                    _context.Roles.Add(model.NuevoRol);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");  // Redirige al listado de roles
                }
                catch (Exception ex)
                {
                    // Si ocurre un error, agrega un mensaje a los errores del modelo
                    ModelState.AddModelError("", $"Ocurrió un error al guardar los datos: {ex.Message}");
                }
            }

            // Si el modelo no es válido, recarga la lista de roles y muestra los errores
            model.ListaRoles = await _context.Roles.ToListAsync();  // Carga los roles existentes
            return View("Index", model);  // Regresa a la vista Index con el modelo actualizado
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreRoles,Descripcion")] Roles rol)
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
                        ModelState.AddModelError("NombreRoles", "Este nombre de rol ya está en uso");
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

        // Acción para eliminar un rol Javier Eulices Martinez
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                return NotFound();
            }

            rol.Activo = !rol.Activo;

            _context.Roles.Update(rol);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool RolExists(int id)
        {
            return _context.Roles.Any(e => e.Id == id);
        }
    }
}


