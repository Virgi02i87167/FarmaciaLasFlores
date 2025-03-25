using Microsoft.AspNetCore.Mvc;
using FarmaciaLasFlores.Models;
using FarmaciaLasFlores.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaciaLasFlores.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Mostrar lista de productos
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos.ToListAsync();
            var viewModel = new ProductosViewModel
            {
                NuevoProducto = new Productos(), // Objeto vacío para el formulario
                ListaProductos = productos
            };
            return View(viewModel);
        }

        // Guardar producto en la base de datos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductosViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Si hay errores, se recarga la lista de productos y se devuelve la vista
                viewModel.ListaProductos = await _context.Productos.ToListAsync();
                return View("Index", viewModel);
            }

            try
            {
                _context.Productos.Add(viewModel.NuevoProducto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ocurrió un error al guardar el producto: {ex.Message}");
            }

            return View("Index", viewModel);
        }
    }
}