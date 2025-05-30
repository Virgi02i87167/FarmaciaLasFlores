﻿using FarmaciaLasFlores.Db;
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
using System.Security.Claims;
using static iTextSharp.text.pdf.AcroFields;

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
            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.Usuario)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();

            var usuarios = await _context.Usuarios.OrderBy(u => u.NombreUsuario).ToListAsync();

            var viewModel = new VentasViewModel
            {
                ListaVentas = ventas,
                ListaUsuarios = usuarios
            };

            ViewBag.Productos = await _context.Productos.ToListAsync(); // opcional

            return View(viewModel);
        }

        // Acción para ver los detalles de productos disponibles
        public async Task<IActionResult> Details(VentasViewModel filtro)
        {
            var productosQuery = _context.Productos
                .Include(p => p.Medicamentos)
                .Where(p => p.Cantidad > 0 && p.FechaVencimiento >= DateTime.Today.AddDays(1))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.SearchString))
            {
                var search = filtro.SearchString.Trim().ToLower();
                productosQuery = productosQuery
                    .Where(p =>
                        p.Nombre.ToLower().Contains(search) ||
                        p.Medicamentos.TipoMedicamento.ToLower().Contains(search));
            }

            filtro.ListaProductos = await productosQuery
                .OrderBy(p => p.FechaRegistro)
                .ToListAsync();

            return View(filtro);
        }

        // Acción para buscar ventas
        public async Task<IActionResult> BuscarVentas(DateTime? fechaInicio, DateTime? fechaFin, int? usuarioId)
        {
            var ventasQuery = _context.Ventas
                .Include(v => v.Detalles)
                .AsQueryable();

            if (fechaInicio.HasValue)
            {
                ventasQuery = ventasQuery.Where(v => v.FechaVenta >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                ventasQuery = ventasQuery.Where(v => v.FechaVenta <= fechaFin.Value);
            }

            if (usuarioId.HasValue && usuarioId > 0)
            {
                ventasQuery = ventasQuery.Where(v => v.UsuarioId == usuarioId.Value);
            }

            var ventas = await ventasQuery.ToListAsync();
            var usuarios = await _context.Usuarios.OrderBy(p => p.NombreUsuario).ToListAsync();

            // Usar ViewBag para popular el dropdown
            ViewBag.Usuarios = usuarios;

            var viewModel = new VentasViewModel
            {
                ListaVentas = ventas,
                ListaUsuarios = usuarios
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
                        PrecioVenta = producto.PrecioVenta,
                        Cantidad = 1
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
        public IActionResult FinalizarVenta(Dictionary<int, int> cantidad, Dictionary<int, decimal> precioVenta)
        {
            try
            {
                var carrito = HttpContext.Session.GetObjectFromJson<List<ItemCarrito>>("Carrito") ?? new List<ItemCarrito>();

                if (carrito == null || !carrito.Any())
                {
                    TempData["ErrorMessage"] = "El carrito está vacío.";
                    return RedirectToAction("Carrito");
                }

                // Obtener el UsuarioId desde la sesión
                int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

                if (usuarioId == 0)
                {
                    TempData["ErrorMessage"] = "Sesión expirada. Por favor, inicie sesión nuevamente.";
                    return RedirectToAction("Index", "Login");
                }

                // Actualizar las cantidades en el carrito según lo enviado por el formulario
                foreach (var item in carrito)
                {
                    if (cantidad.ContainsKey(item.ProductoId))
                    {
                        item.Cantidad = cantidad[item.ProductoId];
                    }
                }

                decimal total = carrito.Sum(item => item.PrecioVenta * item.Cantidad);

                var nuevaVenta = new Ventas
                {
                    FechaVenta = DateTime.Now,
                    UsuarioId = usuarioId,
                    Total = total,
                    Estado = true,
                    Detalles = new List<DetalleVenta>()
                };

                foreach (var item in carrito)
                {
                    var producto = _context.Productos.FirstOrDefault(p => p.Id == item.ProductoId);
                    if (producto == null)
                    {
                        TempData["ErrorMessage"] = $"El producto con ID {item.ProductoId} no existe.";
                        return RedirectToAction("Carrito");
                    }

                    if (producto.Cantidad < item.Cantidad)
                    {
                        TempData["ErrorMessage"] = $"No hay suficiente inventario para el producto '{producto.Nombre}'. Disponible: {producto.Cantidad}, Solicitado: {item.Cantidad}.";
                        return RedirectToAction("Carrito");
                    }

                    // Disminuir la cantidad del inventario
                    producto.Cantidad -= item.Cantidad;

                    nuevaVenta.Detalles.Add(new DetalleVenta
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioVenta = item.PrecioVenta,
                        Subtotal = item.PrecioVenta * item.Cantidad
                    });

                    _context.Productos.Update(producto);
                }

                _context.Ventas.Add(nuevaVenta);
                _context.SaveChanges();

                HttpContext.Session.Remove("Carrito");

                TempData["SuccessMessage"] = $"Venta #{nuevaVenta.Id} realizada correctamente el {nuevaVenta.FechaVenta:dd/MM/yyyy HH:mm}.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al procesar la venta: " + ex.Message;
                return RedirectToAction("Carrito");
            }
        }

        int ObtenerUsuarioId()
        {
            return HttpContext.Session.GetInt32("UsuarioId") ?? 0;
        }

        public ActionResult Editar(int id)
        {
            var venta = _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefault(v => v.Id == id);

            if (venta == null)
                return HttpNotFound();

            var model = new EditarVentaViewModel
            {
                VentaId = venta.Id,
                Detalles = venta.Detalles.Select(d => new DetalleVentaViewModel
                {
                    DetalleId = d.Id,
                    ProductoId = d.ProductoId,
                    NombreProducto = d.Producto.Nombre,
                    Cantidad = d.Cantidad,
                    PrecioVenta = d.PrecioVenta
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarCambiosVenta(EditarVentaViewModel model)
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine("Si llegas");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ErrorMessage"] = "Errores de validación: " + string.Join(", ", errors);
                return View("Editar", model);
            }

            var venta = _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefault(v => v.Id == model.VentaId);

            if (venta == null)
            {
                TempData["ErrorMessage"] = $"No se encontró la venta con ID {model.VentaId}.";
                return RedirectToAction("Index");
            }

            if (model.Detalles == null || !model.Detalles.Any())
            {
                TempData["ErrorMessage"] = "No se recibieron detalles de la venta.";
                return View("Editar", model);
            }

            foreach (var detalleVM in model.Detalles)
            {
                if (detalleVM.PrecioVenta <= 0 || detalleVM.Cantidad <= 0)
                {
                    TempData["ErrorMessage"] = "El precio y la cantidad deben ser mayores que cero.";
                    return View("Editar", model);
                }

                var detalleOriginal = venta.Detalles.FirstOrDefault(d => d.Id == detalleVM.DetalleId);
                if (detalleOriginal == null)
                {
                    TempData["ErrorMessage"] = $"Detalle con ID {detalleVM.DetalleId} no encontrado.";
                    return View("Editar", model);
                }

                // Obtener producto actual
                var producto = _context.Productos.FirstOrDefault(p => p.Id == detalleOriginal.ProductoId);
                if (producto == null)
                {
                    TempData["ErrorMessage"] = $"Producto con ID {detalleOriginal.ProductoId} no encontrado.";
                    return View("Editar", model);
                }

                // Restaurar al stock la cantidad anterior
                producto.Cantidad += detalleOriginal.Cantidad;

                // Verificar si hay suficiente stock para la nueva cantidad
                if (producto.Cantidad < detalleVM.Cantidad)
                {
                    TempData["ErrorMessage"] = $"No hay suficiente stock disponible para el producto '{producto.Nombre}'. Disponible: {producto.Cantidad}, solicitado: {detalleVM.Cantidad}.";
                    return View("Editar", model);
                }

                // Aplicar nueva cantidad y precio
                producto.Cantidad -= detalleVM.Cantidad;
                detalleOriginal.Cantidad = detalleVM.Cantidad;
                detalleOriginal.PrecioVenta = detalleVM.PrecioVenta;
                detalleOriginal.Subtotal = detalleVM.PrecioVenta * detalleVM.Cantidad;

                // Actualizar el producto en el contexto
                _context.Productos.Update(producto);
            }

            venta.Total = venta.Detalles.Sum(d => d.Subtotal);

            try
            {
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Cambios guardados exitosamente.";
                return RedirectToAction("Index", "Ventas");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al guardar los cambios: {ex.Message}";
                return View("Editar", model);
            }
        }

        private ActionResult HttpNotFound()
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AnularVenta(int ventaId)
        {
            try
            {
                var venta = _context.Ventas
                    .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                    .FirstOrDefault(v => v.Id == ventaId);

                if (venta == null)
                {
                    TempData["ErrorMessage"] = "Venta no encontrada.";
                    return RedirectToAction("Index");
                }

                // Validar si la venta tiene más de 3 días
                if (venta.FechaVenta.AddDays(3) < DateTime.Now)
                {
                    TempData["ErrorMessage"] = "No se puede anular una venta que tiene más de 3 días.";
                    return RedirectToAction("Index");
                }

                // Devolver los productos al inventario
                foreach (var detalle in venta.Detalles)
                {
                    var producto = detalle.Producto;
                    if (producto != null)
                    {
                        producto.Cantidad += detalle.Cantidad;
                        _context.Productos.Update(producto);
                    }
                }

                // Cambiar el estado de la venta
                venta.Estado = false;
                _context.Ventas.Update(venta);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Venta anulada correctamente.";
                return RedirectToAction("IndexAnuladas");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al anular la venta: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> IndexAnuladas()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.Usuario)
                .Where(v => v.Estado == false)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();

            var usuarios = await _context.Usuarios.OrderBy(u => u.NombreUsuario).ToListAsync();

            var viewModel = new VentasViewModel
            {
                ListaVentas = ventas,
                ListaUsuarios = usuarios
            };

            return View(viewModel);
        }

        public async Task<IActionResult> DetallesVenta(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        public async Task<IActionResult> Anular(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarAnular(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                return NotFound();
            }

            // Validar si la venta tiene más de 3 días
            if (venta.FechaVenta.AddDays(3) < DateTime.Now)
            {
                TempData["ErrorMessage"] = "No se puede anular una venta que tiene más de 3 días.";
                return RedirectToAction("Index");
            }

            // Devolver los productos al inventario
            foreach (var detalle in venta.Detalles)
            {
                var producto = detalle.Producto;
                if (producto != null)
                {
                    producto.Cantidad += detalle.Cantidad;
                    _context.Productos.Update(producto);
                }
            }

            // Cambiar el estado de la venta
            venta.Estado = false;
            _context.Ventas.Update(venta);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Venta anulada correctamente.";
            return RedirectToAction("IndexAnuladas");
        }
    }
}
