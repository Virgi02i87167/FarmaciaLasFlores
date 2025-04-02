using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaciaLasFlores.Controllers
{
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Variable para almacenar el carrito temporalmente
        private static List<Productos> carrito = new List<Productos>();

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Acción para mostrar la vista de productos disponibles
        public async Task<IActionResult> Index()
        {
            // Obtener los productos disponibles y ordenarlos por FechaRegistro
            var productos = await _context.Productos
                                          .OrderBy(p => p.FechaRegistro)
                                          .ToListAsync() ?? new List<Productos>(); // Asegurar que no sea null

            // Obtener las ventas realizadas y cargar la relación con los productos
            var ventas = await _context.Ventas.Include(v => v.Producto).ToListAsync();

            // Crear el ViewModel y asignar los datos correspondientes
            var viewModel = new VentasViewModel
            {
                ListaProductos = productos, // Lista de productos ordenados por fecha
                ListaVentas = ventas // Lista de ventas realizadas
            };

            ViewBag.Productos = productos; // Reutilizar la lista de productos ya obtenida
            return View(viewModel); // Pasar el ViewModel a la vista
        }


        // Consulta para buscar ventas// Javier Eulices Martinez
        public async Task<IActionResult> BuscarVentas(DateTime? fechaInicio, DateTime? fechaFin, int? productoId)
        {
            var ventasQuery = _context.Ventas.Include(v => v.Producto).AsQueryable();

            if (fechaInicio.HasValue)
            {
                ventasQuery = ventasQuery.Where(v => v.FechaVenta >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                ventasQuery = ventasQuery.Where(v => v.FechaVenta <= fechaFin.Value);
            }

            if (productoId.HasValue && productoId.Value > 0)
            {
                ventasQuery = ventasQuery.Where(v => v.ProductoId == productoId.Value);
            }

            var ventas = await ventasQuery.ToListAsync();

            var viewModel = new VentasViewModel
            {
                ListaVentas = ventas
            };

            return View("Index", viewModel); // Pasar el ViewModel a la vista
        }


        // Acción para agregar un producto al carrito
        [HttpPost]
        public IActionResult AddToCart(int ProductoId)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.Id == ProductoId);
            if (producto != null)
            {
                carrito.Add(producto); // Agregar el producto al carrito
                TempData["SuccessMessage"] = "Producto agregado al carrito."; // Mensaje de éxito al agregar
            }
            return RedirectToAction("Carrito");
        }

        // Acción para ver el carrito
        public IActionResult Carrito()
        {
            var viewModel = new VentasViewModel
            {
                ListaProductos = carrito
            };
            return View(viewModel);
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

                    // Crear el objeto de venta
                    var venta = new Ventas
                    {
                        ProductoId = producto.Id,
                        FechaVenta = DateTime.Now,
                        Cantidad = cantidadValor,
                        PrecioVenta = precioValor,
                        Total = precioValor * cantidadValor
                    };

                    ventasARegistrar.Add(venta); // Agregar la venta a la lista de ventas a registrar

                    // Restar la cantidad vendida del inventario
                    productoInventario.Cantidad -= cantidadValor;
                    _context.Update(productoInventario); // Actualizar el inventario
                }

                // Guardar todas las ventas en la base de datos
                await _context.Ventas.AddRangeAsync(ventasARegistrar);

                // Guardar los cambios en la base de datos
                int cambios = await _context.SaveChangesAsync();
                Console.WriteLine($"Filas afectadas en la BD: {cambios}"); // Imprimir filas afectadas para depuración

                carrito.Clear(); // Limpiar el carrito después de la venta
                TempData["SuccessMessage"] = "Venta registrada exitosamente."; // Mensaje de éxito de la venta
                return RedirectToAction(nameof(Index)); // Redirigir al índice después de la venta exitosa
            }
            catch (Exception ex)
            {
                // En caso de error, se maneja la excepción y se muestra el error
                Console.WriteLine($"Error en FinalizarVenta: {ex.Message}");
                ModelState.AddModelError("", "Error al registrar la venta.");
                return View("Carrito", viewModel); // Regresar al carrito si hay error
            }
        }
    }
}
