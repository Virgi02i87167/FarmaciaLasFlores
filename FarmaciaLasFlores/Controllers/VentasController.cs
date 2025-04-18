using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
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
        private readonly ILogger<VentasController> _logger;

        // Variable para almacenar el carrito temporalmente
        private static List<Productos> carrito = new List<Productos>();

        public VentasController(ApplicationDbContext context, ILogger<VentasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Acción para mostrar la vista de productos disponibles
        public async Task<IActionResult> Index()
        {
            // Obtener los productos disponibles y ordenarlos por FechaRegistro
            var productos = await _context.Productos
                                          .OrderBy(p => p.FechaRegistro)
                                          .ToListAsync() ?? new List<Productos>(); // Asegurar que no sea null

            // Obtener las ventas realizadas y cargar la relacion con los productos
            //var ventas = await _context.Ventas.Include(v => v.Producto).OrderByDescending(v => v.FechaVenta) // Ordenar las ventas por fecha (más reciente a más antigua).ToListAsync();

            var viewModel = new VentasViewModel
            {
                ListaProductos = productos, // Lista de productos ordenados por fecha
               //ListaVentas = ventas // Lista de ventas ordenadas por fecha de venta
            };

            ViewBag.Productos = productos; // Reutilizar la lista de productos ya obtenida
            return View(viewModel); // Pasar el ViewModel a la vista
        }



        // Consulta para buscar ventas// Javier Eulices Martinez
        public async Task<IActionResult> BuscarVentas(DateTime? fechaInicio, DateTime? fechaFin, int? productoId)
        {
            //var ventasQuery = _context.Ventas.Include(v => v.Producto).AsQueryable();

            if (fechaInicio.HasValue)
            {
                //ventasQuery = ventasQuery.Where(v => v.FechaVenta >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                //ventasQuery = ventasQuery.Where(v => v.FechaVenta <= fechaFin.Value);
            }

            if (productoId.HasValue && productoId.Value > 0)
            {
                //ventasQuery = ventasQuery.Where(v => v.ProductoId == productoId.Value);
            }

            //var ventas = await ventasQuery.ToListAsync();

            //var productos = await _context.Productos.OrderBy(p => p.FechaRegistro).ToListAsync() ?? new List<Productos>();

            var viewModel = new VentasViewModel
            {
               //ListaProductos = productos,
                //ListaVentas = ventas
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
                        FechaVenta = DateTime.Now,
                        //Cantidad = cantidadValor,
                        //PrecioVenta = precioValor,
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

        
        // Acción para mostrar el formulario de edición
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            //var venta = await _context.Ventas.Include(v => v.Producto).FirstOrDefaultAsync(v => v.Id == id);

            //if (venta == null)
            {
                return NotFound();
            }

            var viewModel = new VentasViewModel
            {
                //NuevaVenta = venta,
                ListaProductos = await _context.Productos.ToListAsync()
            };

            return View(viewModel);
        }


        // Acción para guardar los cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(VentasViewModel viewModel)
        {
            Console.WriteLine("Entró al método Editar (POST)");

            if (!ModelState.IsValid)
            {
                var venta = await _context.Ventas.FindAsync(viewModel.NuevaVenta.Id);
                if (venta == null)
                {
                    return NotFound();
                }


                try
                {
                    _context.Update(venta);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venta actualizada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar los cambios: " + ex.Message);
                }
            }

            // Recargar productos si hay errores
            viewModel.ListaProductos = await _context.Productos.ToListAsync();
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteVenta(int id)
        {
            try
            {
                //var venta = _context.Ventas.Include(v => v.Producto).FirstOrDefault(v => v.Id == id);

                //if (venta == null)
                {
                    return Json(new { success = false, message = "Venta no encontrada" });
                }

                // Aquí puedes agregar validaciones adicionales si es necesario
                // Por ejemplo, no permitir eliminar ventas antiguas:
                //if (venta.FechaVenta < DateTime.Now.AddMonths(-1))
                {
                    return Json(new { success = false, message = "No se pueden eliminar ventas con más de 1 mes de antigüedad" });
                }

                //_context.Ventas.Remove(venta);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Venta eliminada correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar venta");
                return Json(new { success = false, message = "Ocurrió un error al eliminar la venta" });
            }
        }
    }
}
