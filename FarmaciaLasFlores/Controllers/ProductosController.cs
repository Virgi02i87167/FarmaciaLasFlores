using Microsoft.AspNetCore.Mvc;
using FarmaciaLasFlores.Models;
using FarmaciaLasFlores.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace FarmaciaLasFlores.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            // Obtener la lista de medicamentos desde la base de datos
            var medicamentos = await _context.Medicamentos.ToListAsync();

            // Consultar los productos, con la posibilidad de filtrar según la búsqueda
            var productosQuery = _context.Productos.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                productosQuery = productosQuery
                                 .Where(p => p.Nombre.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                                            p.Lote.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            // Incluir medicamentos relacionados
            productosQuery = productosQuery.Include(p => p.Medicamentos);

            var productos = await productosQuery.ToListAsync();

            // Crear el modelo de vista
            var viewModel = new ProductosViewModel
            {
                NuevoProducto = new Productos(), // Objeto vacío para el formulario
                ListaProductos = productos // Lista de productos con Medicamentos relacionados
            };

            // Asignar el valor de búsqueda a la vista para mantener el valor de búsqueda
            ViewData["SearchString"] = searchString;

            // Cargar las listas desplegables
            CargarListasDesplegables();

            return View(viewModel);
        }

        private void CargarListasDesplegables()
        {
            // Cargar la lista de medicamentos en el ViewData para la lista desplegable
            ViewData["MedicamentosId"] = new SelectList(_context.Medicamentos, "Id", "TipoMedicamento");
        }

        // Guardar nuevo producto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductosViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Imprimir el estado del modelo para depuración
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Count > 0)
                    {
                        Console.WriteLine($"Campo: {key}");
                        foreach (var error in state.Errors)
                        {
                            Console.WriteLine($" - Error: {error.ErrorMessage}");
                        }
                    }
                }
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

            if (viewModel.NuevoProducto.MedicamentosId <= 0)
            {
                ModelState.AddModelError("NuevoProducto.MedicamentosId", "El tipo de medicamento no puede estar vacío.");
            }

            try
            {
                viewModel.NuevoProducto.FechaRegistro = DateTime.Now; // Se asigna la fecha actual al registrar

                _context.Productos.Add(viewModel.NuevoProducto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Redirige al índice después de guardar
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar el producto: {ex.Message}");
                viewModel.ListaProductos = await _context.Productos
                    .Include(p => p.Medicamentos)
                    .ToListAsync();
                CargarListasDesplegables();
                return View("Index", viewModel);
            }
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
                producto.MedicamentosId = viewModel.NuevoProducto.MedicamentosId;

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

        //Eliminacion de producto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var producto = _context.Productos.Find(id);
                if (producto == null)
                {
                    return NotFound();
                }

                _context.Productos.Remove(producto);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Producto eliminado correctamente";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el producto: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}