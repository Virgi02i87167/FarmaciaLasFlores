using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Helpers;
using FarmaciaLasFlores.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaLasFlores.Controllers
{
    [AuthorizeRoles(RolesSistema.Administrador)]
    public class PermisosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PermisosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Asignar(int rolId)
        {
            var permisosDisponibles = new List<string> { "VerUsuarios", "VerProductos", "VerRoles", "VerTipoMedicamentos", "VerReportes", "VerVentas", "AsignarPermisos" };

            var permisosActuales = await _context.Permisos
                .Where(p => p.RolId == rolId)
                .Select(p => p.Nombre)
                .ToListAsync();

            var modelo = new PermisoAsignarViewModel
            {
                RolId = rolId,
                Permisos = permisosDisponibles.Select(nombre => new PermisoSeleccionado
                {
                    PermisoId = nombre,
                    Nombre = nombre,
                    Seleccionado = permisosActuales.Contains(nombre)
                }).ToList()
            };

            ViewBag.NombreRol = _context.Roles.FirstOrDefault(r => r.Id == rolId)?.NombreRoles;
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Asignar(PermisoAsignarViewModel modelo)
        {
            var permisosExistentes = await _context.Permisos
                .Where(p => p.RolId == modelo.RolId)
                .ToListAsync();

            _context.Permisos.RemoveRange(permisosExistentes);

            var permisosSeleccionados = modelo.Permisos
                .Where(p => p.Seleccionado)
                .Select(p => new Permiso
                {
                    RolId = modelo.RolId,
                    Nombre = p.Nombre
                });

            await _context.Permisos.AddRangeAsync(permisosSeleccionados);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Roles");
        }
    }
}
