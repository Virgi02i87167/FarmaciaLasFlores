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
                                          .OrderBy(p => p.FechaRegistro) // Ordenar por Fecha de Registro
                                          .ToListAsync();

            // Obtener las ventas realizadas y cargar la relación con los productos
            var ventas = await _context.Ventas.Include(v => v.Producto).ToListAsync();

            // Crear el ViewModel y asignar los datos correspondientes
            var viewModel = new VentasViewModel
            {
                ListaProductos = productos, // Lista de productos ordenados por fecha
                ListaVentas = ventas // Lista de ventas realizadas
            };

            return View(viewModel); // Pasar el ViewModel a la vista
        }

        // Consulta para buscar ventas// Javier Eulices Martinez
        public async Task<IActionResult> BuscarVentas(DateTime? fechaInicio, DateTime? fechaFin)
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
                // Agregar producto al carrito
                carrito.Add(producto);
                return RedirectToAction("Carrito");
            }

            return RedirectToAction("Index");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarVenta(VentasViewModel viewModel)
        {
            try
            {
                foreach (var producto in viewModel.ListaProductos)
                {
                    var cantidad = Request.Form["cantidad[" + producto.Id + "]"].FirstOrDefault();
                    var precioVenta = Request.Form["precioVenta[" + producto.Id + "]"].FirstOrDefault();

                    if (int.TryParse(cantidad, out int cantidadValor) && decimal.TryParse(precioVenta, out decimal precioValor))
                    {
                        if (cantidadValor < 1)
                        {
                            ModelState.AddModelError("", "La cantidad del producto no es válida.");
                            return View("Carrito", viewModel);
                        }

                        var venta = new Ventas
                        {
                            ProductoId = producto.Id,
                            FechaVenta = DateTime.Now,
                            Cantidad = cantidadValor,
                            PrecioVenta = precioValor,
                            Total = precioValor * cantidadValor
                        };

                        _context.Ventas.Add(venta); // Agregar la venta

                        var productoInventario = await _context.Productos.FirstOrDefaultAsync(p => p.Id == producto.Id);
                        if (productoInventario != null)
                        {
                            productoInventario.Cantidad -= cantidadValor; // Restar la cantidad vendida
                            _context.Update(productoInventario); // Actualizar inventario
                        }
                    }
                }

                await _context.SaveChangesAsync(); // Guardar cambios en la base de datos

                carrito.Clear(); // Limpiar el carrito
                TempData["SuccessMessage"] = "Venta registrada exitosamente."; // Mostrar mensaje de éxito
                return RedirectToAction(nameof(Index)); // Redirigir a la vista de productos disponibles
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al registrar la venta: {ex.Message}");
                return View("Carrito", viewModel); // Regresar al carrito si hay error
            }
        }

    }
}
