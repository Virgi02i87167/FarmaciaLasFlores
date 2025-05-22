using Microsoft.AspNetCore.Mvc;
using FarmaciaLasFlores.Models;
using FarmaciaLasFlores.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using FarmaciaLasFlores.Helpers;

namespace FarmaciaLasFlores.Controllers
{
    [AuthorizeRoles("Administrador", "Supervisor")]
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(ProductosViewModel filtro)
        {
            var productosQuery = _context.Productos
                .Include(p => p.Medicamentos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.SearchString))
            {
                var search = filtro.SearchString.Trim().ToLower();

                productosQuery = productosQuery
                    .Where(p =>
                        p.Nombre.ToLower().Contains(search) ||
                        p.Medicamentos.TipoMedicamento.ToLower().Contains(search));
            }

            var productos = await productosQuery.ToListAsync();

            filtro.ListaProductos = productos;
            filtro.NuevoProducto = new Productos();

            return View(filtro);
        }


        private void CargarListasDesplegables()
        {
            // Cargar solo los medicamentos activos
            var medicamentosActivos = _context.Medicamentos
                .Where(m => m.Estado)
                .ToList();

            // Usar la lista filtrada en el ViewData
            ViewData["MedicamentosId"] = new SelectList(medicamentosActivos, "Id", "TipoMedicamento");
        }

        public IActionResult Create()
        {
            CargarListasDesplegables();
            return View();
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
            if (viewModel.NuevoProducto.PrecioCompra <= 0)
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

                // 👉 Calcular el precio de venta con el 30% de ganancia
                viewModel.NuevoProducto.PrecioVenta = viewModel.NuevoProducto.PrecioCompra * 1.30m;

                _context.Productos.Add(viewModel.NuevoProducto);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Producto Agregado correctamente.";
                return RedirectToAction("Index");
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
            CargarListasDesplegables();
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            producto.PrecioVenta = producto.PrecioCompra * 1.30m;

            var viewModel = new ProductosViewModel
            {
                NuevoProducto = producto,
                ListaProductos = await _context.Productos
                .Include(p => p.Medicamentos)
                .ToListAsync()
            };

            return View(viewModel); // Usa la misma vista con datos cargados
        }

        // Actualizar producto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductosViewModel viewModel)
        {

            if (ModelState.IsValid)
            {
                CargarListasDesplegables();
                viewModel.ListaProductos = await _context.Productos
                    .Include(p => p.Medicamentos)
                    .ToListAsync();
                return View(viewModel);
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
                producto.PrecioCompra = viewModel.NuevoProducto.PrecioCompra;
                producto.FechaVencimiento = viewModel.NuevoProducto.FechaVencimiento;
                producto.Lote = viewModel.NuevoProducto.Lote;
                producto.MedicamentosId = viewModel.NuevoProducto.MedicamentosId;
                producto.PrecioVenta = producto.PrecioCompra * 1.30m;
                producto.Estado = viewModel.NuevoProducto.Estado;

                _context.Update(producto);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "El producto fue actualizado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al modificar el producto: {ex.Message}");
            }

            CargarListasDesplegables(); // También aquí
            viewModel.ListaProductos = await _context.Productos
                .Include(p => p.Medicamentos)
                .ToListAsync();
            return View(viewModel);
        }

        public IActionResult Delete(int id)
        {
            var producto = _context.Productos
        .Include(p => p.Medicamentos) // Para incluir TipoMedicamento
        .FirstOrDefault(p => p.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        //Eliminacion de producto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deleted(int id)
        {
            var producto = _context.Productos.Find(id);
            if (producto == null)
            {
                return NotFound();
            }

            producto.Estado = false;
            _context.SaveChanges();
            TempData["Mensaje"] = "Producto desactivado correctamente.";
            return RedirectToAction("Index");
        }
    }
}