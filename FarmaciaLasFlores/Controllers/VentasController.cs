using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Helpers;
using FarmaciaLasFlores.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaciaLasFlores.Controllers
{
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VentasController> _logger;
        private static List<Productos> carrito = new List<Productos>();

        public VentasController(ApplicationDbContext context, ILogger<VentasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Acción para mostrar la vista de productos disponibles
        public async Task<IActionResult> Index()
        {
            var viewModel = new VentasViewModel
            {
                ListaVentas = await _context.Ventas
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(v => v.Usuario)
            .OrderByDescending(v => v.FechaVenta)
            .ToListAsync()
            };

            // Cargar los productos para el filtro (si los necesitas en el ViewBag)
            ViewBag.Productos = await _context.Productos.ToListAsync();

            return View(viewModel);
        }

        // Acción para ver los detalles de productos disponibles
        public async Task<IActionResult> Details()
        {
            var productos = await _context.Productos
                .Include(p => p.Medicamentos)
                .OrderBy(p => p.FechaRegistro)
                .ToListAsync();

            var modelo = new VentasViewModel
            {
                ListaProductos = productos ?? new List<Productos>()
            };

            return View(modelo);
        }

        // Acción para buscar ventas
        public async Task<IActionResult> BuscarVentas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var ventasQuery = _context.Ventas.Include(v => v.Detalles).AsQueryable();

            if (fechaInicio.HasValue)
            {
                ventasQuery = ventasQuery.Where(v => v.FechaVenta >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                ventasQuery = ventasQuery.Where(v => v.FechaVenta <= fechaFin.Value);
            }

            var ventas = await ventasQuery.ToListAsync();
            var productos = await _context.Productos.OrderBy(p => p.FechaRegistro).ToListAsync() ?? new List<Productos>();

            var viewModel = new VentasViewModel
            {
                ListaProductos = productos,
                ListaVentas = ventas
            };

            return View("Index", viewModel);
        }


        // Acción para ver el carrito
        public IActionResult Carrito()
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<ItemCarrito>>("Carrito") ?? new List<ItemCarrito>();

            var viewModel = new VentasViewModel
            {
                ListaCarrito = carrito
            };

            return View(viewModel);
        }

        // Acción para agregar un producto al carrito
        [HttpPost]
        public IActionResult AddToCart(int ProductoId)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.Id == ProductoId);
            if (producto == null)
            {
                TempData["ErrorMessage"] = "Producto no encontrado.";
                return RedirectToAction("Details");
            }

            // Obtener carrito actual de la sesión
            var carrito = HttpContext.Session.GetObjectFromJson<List<ItemCarrito>>("Carrito") ?? new List<ItemCarrito>();

            // Ver si el producto ya está en el carrito
            var itemExistente = carrito.FirstOrDefault(i => i.ProductoId == ProductoId);
            if (itemExistente != null)
            {
                // Validar stock disponible antes de incrementar
                if (itemExistente.Cantidad < producto.Cantidad)
                {
                    itemExistente.Cantidad++; // Incrementar cantidad si hay stock
                    TempData["SuccessMessage"] = "Cantidad incrementada en el carrito.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No hay suficiente stock para este producto.";
                }
            }
            else
            {
                if (producto.Cantidad > 0)
                {
                    // Agregar nuevo item al carrito
                    carrito.Add(new ItemCarrito
                    {
                        ProductoId = producto.Id,
                        Nombre = producto.Nombre,
                        PrecioVenta = producto.PrecioVenta
                    });
                    TempData["SuccessMessage"] = "Producto agregado al carrito.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Producto sin stock disponible.";
                }
            }

            // Guardar carrito actualizado en la sesión
            HttpContext.Session.SetObjectAsJson("Carrito", carrito);

            return RedirectToAction("Carrito");
        }

        [HttpPost]
        public IActionResult EliminarDelCarritos([FromBody] ProductoRequest request)
        {
            var productoId = request.ProductoId;
            var carrito = HttpContext.Session.GetObjectFromJson<List<ItemCarrito>>("Carrito") ?? new List<ItemCarrito>();
            var eliminado = carrito.RemoveAll(p => p.ProductoId == productoId) > 0;

            HttpContext.Session.SetObjectAsJson("Carrito", carrito);

            // Si el producto se eliminó correctamente, se devuelve un OK (200)
            return eliminado ? Ok() : BadRequest("No se pudo eliminar el producto.");
        }

        public class ProductoRequest
        {
            public int ProductoId { get; set; }
        }

        [HttpPost]
        public IActionResult LimpiarCarrito()
        {
            HttpContext.Session.Remove("Carrito");
            return Ok();
        }

        // Acción para finalizar la venta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarVenta(VentasViewModel viewModel)
        {
            try
            {
                // Verificar si el carrito está vacío
                if (!carrito.Any())
                {
                    ModelState.AddModelError("", "El carrito está vacío.");
                    return View("Carrito", viewModel);
                }

                var ventasARegistrar = new List<Ventas>();

                // Iterar sobre los productos en el carrito
                foreach (var producto in carrito)
                {
                    // Obtener cantidad y precio desde el formulario
                    var cantidad = Request.Form["cantidad[" + producto.Id + "]"].FirstOrDefault();
                    var precioVenta = Request.Form["precioVenta[" + producto.Id + "]"].FirstOrDefault();

                    // Validar que la cantidad sea válida
                    if (!int.TryParse(cantidad, out int cantidadValor) || cantidadValor < 1)
                    {
                        ModelState.AddModelError("", $"La cantidad del producto {producto.Nombre} no es válida.");
                        return View("Carrito", viewModel);
                    }

                    // Validar que el precio sea válido
                    if (!decimal.TryParse(precioVenta, out decimal precioValor) || precioValor <= 0)
                    {
                        ModelState.AddModelError("", $"El precio del producto {producto.Nombre} no es válido.");
                        return View("Carrito", viewModel);
                    }

                    // Verificar si el producto tiene suficiente stock
                    var productoInventario = await _context.Productos.FirstOrDefaultAsync(p => p.Id == producto.Id);
                    if (productoInventario == null || productoInventario.Cantidad < cantidadValor)
                    {
                        ModelState.AddModelError("", $"No hay suficiente stock para el producto {producto.Nombre}.");
                        return View("Carrito", viewModel);
                    }

                    // Restar la cantidad vendida del inventario
                    productoInventario.Cantidad -= cantidadValor;
                    _context.Update(productoInventario); // Actualizar el inventario
                }

                // Guardar todas las ventas en la base de datos
                await _context.Ventas.AddRangeAsync(ventasARegistrar);

                // Guardar los cambios en la base de datos
                int cambios = await _context.SaveChangesAsync();
                Console.WriteLine($"Filas afectadas en la BD: {cambios}");

                carrito.Clear(); // Limpiar el carrito después de la venta
                TempData["SuccessMessage"] = "Venta registrada exitosamente."; // Mensaje de éxito de la venta
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en FinalizarVenta: {ex.Message}");
                ModelState.AddModelError("", "Error al registrar la venta.");
                return View("Carrito", viewModel);
            }
        }
    }
}
