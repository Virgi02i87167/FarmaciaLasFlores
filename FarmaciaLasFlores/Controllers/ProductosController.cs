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

        // Mostrar lista de productos con búsqueda
        public async Task<IActionResult> Index(string searchString)
        {
            var productos = from p in _context.Productos select p;

            if (!string.IsNullOrEmpty(searchString))
            {
                productos = productos.Where(p => p.Nombre.Contains(searchString) || p.Lote.Contains(searchString));
            }

            var viewModel = new ProductosViewModel
            {
                NuevoProducto = new Productos(), // Objeto vacío para el formulario
                ListaProductos = await productos.ToListAsync()
            };

            ViewData["SearchString"] = searchString;
            return View(viewModel);
        }

        // Guardar nuevo producto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductosViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.ListaProductos = await _context.Productos.ToListAsync();
                return View("Index", viewModel);
            }

            // Validaciones adicionales
            if (viewModel.NuevoProducto.Precio <= 0)
            {
                ModelState.AddModelError("NuevoProducto.Precio", "El precio debe ser mayor que cero.");
            }

            if (viewModel.NuevoProducto.Cantidad <= 0)
            {
                ModelState.AddModelError("NuevoProducto.Cantidad", "La cantidad debe ser mayor que cero.");
            }

            if (viewModel.NuevoProducto.FechaVencimiento <= DateTime.Today)
            {
                ModelState.AddModelError("NuevoProducto.FechaVencimiento", "La fecha de vencimiento debe ser futura.");
            }

            if (string.IsNullOrWhiteSpace(viewModel.NuevoProducto.Lote))
            {
                ModelState.AddModelError("NuevoProducto.Lote", "El lote no puede estar vacío.");
            }

            if (string.IsNullOrWhiteSpace(viewModel.NuevoProducto.TipoMedicamento))
            {
                ModelState.AddModelError("NuevoProducto.TipoMedicamento", "El tipo de medicamento no puede estar vacío.");
            }

            // Si hay errores, devolvemos la vista con los mensajes
            if (!ModelState.IsValid)
            {
                viewModel.ListaProductos = await _context.Productos.ToListAsync();
                return View("Index", viewModel);
            }

            try
            {
                viewModel.NuevoProducto.FechaRegistro = DateTime.Now; // Se asigna la fecha actual al registrar

                _context.Productos.Add(viewModel.NuevoProducto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar el producto: {ex.Message}");
            }

            return View("Index", viewModel);
        }

        // Cargar datos en el formulario para editar
        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            var viewModel = new ProductosViewModel
            {
                NuevoProducto = producto,
                ListaProductos = await _context.Productos.ToListAsync()
            };

            return View("Index", viewModel); // Usa la misma vista con datos cargados
        }

        // Actualizar producto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProductosViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.ListaProductos = await _context.Productos.ToListAsync();
                return View("Index", viewModel);
            }

            var producto = await _context.Productos.FindAsync(viewModel.NuevoProducto.Id);
            if (producto == null)
            {
                return NotFound();
            }

            try
            {
                producto.Nombre = viewModel.NuevoProducto.Nombre;
                producto.Cantidad = viewModel.NuevoProducto.Cantidad;
                producto.Precio = viewModel.NuevoProducto.Precio;
                producto.FechaVencimiento = viewModel.NuevoProducto.FechaVencimiento;
                producto.Lote = viewModel.NuevoProducto.Lote;
                producto.TipoMedicamento = viewModel.NuevoProducto.TipoMedicamento;

                _context.Update(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al modificar el producto: {ex.Message}");
            }

            return View("Index", viewModel);
        }
    }
}