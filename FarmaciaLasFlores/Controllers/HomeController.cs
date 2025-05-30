using System.Diagnostics;
using FarmaciaLasFlores.Db;
using System.Globalization;
using FarmaciaLasFlores.Helpers;
using FarmaciaLasFlores.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaLasFlores.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var totalUsuarios = _context.Usuarios.Count(); 
            ViewBag.TotalUsuarios = totalUsuarios;

            var fechaActual = DateTime.Today;
            var fechaLimite = fechaActual.AddDays(3);

            var productosPorVencer = _context.Productos
                .Where(p => p.FechaVencimiento <= fechaLimite && p.FechaVencimiento >= fechaActual && p.Estado)
                .ToList();

            ViewBag.ProductosPorVencer = productosPorVencer.Count;

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (!usuarioId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var usuario = _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefault(u => u.Id == usuarioId.Value);

            if (usuario == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.RolUsuario = usuario.Rol.NombreRoles;

            // Obtener permisos asociados al rol del usuario
            var permisos = _context.Permisos
    .Where(p => p.RolId == usuario.RolId)
    .Select(p => p.Nombre) // O lo que necesites del permiso
    .ToList();

            ViewBag.Permisos = permisos;

            return View();
        }

        [HttpGet]
        public IActionResult ObtenerVentasPorMes()
        {
            var ventas = _context.Ventas
                .Where(v => v.Estado == true)
                .AsEnumerable() // Esto hace que lo siguiente se ejecute en memoria
                .GroupBy(v => new { Anio = v.FechaVenta.Year, Mes = v.FechaVenta.Month })
                .Select(g => new
                {
                    Anio = g.Key.Anio,
                    MesNumero = g.Key.Mes,
                    TotalVentas = g.Count()
                })
                .OrderBy(g => g.Anio)
                .ThenBy(g => g.MesNumero)
                .ToList();

            var culturaEs = new CultureInfo("es-ES");
            var datosConNombreMes = ventas.Select(d => new
            {
                Mes = new DateTime(d.Anio, d.MesNumero, 1).ToString("MMMM", culturaEs),
                TotalVentas = d.TotalVentas
            });

            return Json(datosConNombreMes);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //Asignar que niega el acceso a la vista
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
