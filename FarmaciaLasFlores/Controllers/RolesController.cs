using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaLasFlores.Controllers
{
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


        // Acción para eliminar un rol Javier Eulices Martinez
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id) //Elemento Update
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                return NotFound();
            }

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));  // Redirige al listado de roles
        }
    }
}


