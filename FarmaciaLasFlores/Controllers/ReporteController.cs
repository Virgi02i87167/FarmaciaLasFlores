using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using FarmaciaLasFlores.Db;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using FarmaciaLasFlores.Models;
using Microsoft.EntityFrameworkCore;
using FarmaciaLasFlores.Servicios;
using QuestPDF.Fluent;
using iTextDocument = iTextSharp.text.Document;
using QuestPDF.Infrastructure;


namespace FarmaciaLasFlores.Controllers
{
    public class ReporteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly VentasService _ventasService;

        public ReporteController(ApplicationDbContext context, VentasService ventasService)
        {
            _context = context;
            _ventasService = ventasService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Productos(FiltroFechasViewModel viewModel)
        {
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult GenerarPDFProductos(DateTime? FechaInicio, DateTime? FechaFin)
        {
            var productos = _context.Productos.Include(p => p.Medicamentos).AsQueryable();

            if (FechaInicio.HasValue)
                productos = productos.Where(p => p.FechaRegistro >= FechaInicio.Value);

            if (FechaFin.HasValue)
                productos = productos.Where(p => p.FechaRegistro <= FechaFin.Value);

            var listaProductos = productos.ToList();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                iTextDocument document = new iTextDocument(PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Reporte de Productos", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(title);
                document.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(7);
                table.WidthPercentage = 100;

                string[] headers = { "Nombre", "Cantidad", "Precio", "Lote", "Fecha Registro", "Fecha Vencimiento", "Tipo de Medicamento" };
                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)))
                    {
                        BackgroundColor = BaseColor.LightGray,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }

                foreach (var producto in listaProductos)
                {
                    table.AddCell(new PdfPCell(new Phrase(producto.Nombre)));
                    table.AddCell(new PdfPCell(new Phrase(producto.Cantidad.ToString())));
                    table.AddCell(new PdfPCell(new Phrase(producto.PrecioCompra.ToString("C"))));
                    table.AddCell(new PdfPCell(new Phrase(producto.Lote)));
                    table.AddCell(new PdfPCell(new Phrase(producto.FechaRegistro.ToString("yyyy-MM-dd"))));
                    table.AddCell(new PdfPCell(new Phrase(producto.FechaVencimiento.ToString("yyyy-MM-dd"))));
                    table.AddCell(new PdfPCell(new Phrase(producto.Medicamentos.TipoMedicamento)));
                }

                document.Add(table);
                document.Close();
                writer.Close();

                memoryStream.Position = 0;
                return File(memoryStream.ToArray(), "application/pdf", "Reporte_Productos.pdf");
            }
        }

        public IActionResult Ventas()
        {
            var usuarios = _context.Usuarios.ToList(); // o desde un servicio
            var model = new ReporteVentasViewModel
            {
                Usuarios = usuarios
            };

            return View(model);
        }


        public IActionResult DescargarPDFVenta(int id)
        {
            var venta = _ventasService.ObtenerVentaConDetalles(id);

            if (venta == null)
            {
                TempData["Mensaje"] = "No hay ventas con ese código de factura.";
                return RedirectToAction("Ventas"); // Cambia "Ventas" si tu acción se llama diferente
            }

            try
            {
                var documento = new ReporteVentasPdf(new List<Ventas> { venta }, $"Factura de Venta #{venta.Id}");
                var pdf = documento.GeneratePdf();

                return File(pdf, "application/pdf", $"Venta_{venta.Id}.pdf");
            }
            catch (Exception ex)
            {
                // Mostrar el error en texto plano para debug
                return Content($"Error al generar PDF: {ex.Message}\n\n{ex.StackTrace}");
            }
        }

        public IActionResult DescargarReporteMensual(int mes, int anio)
        {
            var ventas = _ventasService.ObtenerVentasPorMes(mes, anio);
            if (!ventas.Any())
            {
                TempData["Mensaje"] = "No hay ventas en ese mes";
                return RedirectToAction("Ventas");
            }

            var documento = new ReporteVentasPdf(ventas, $"Reporte de Ventas - {mes}/{anio}");
            var pdf = documento.GeneratePdf();

            return File(pdf, "application/pdf", $"Ventas_{mes}_{anio}.pdf");
        }

        public IActionResult DescargarReporteUsuario(int usuarioId)
        {
            var ventas = _ventasService.ObtenerVentasPorUsuario(usuarioId);
            if (!ventas.Any())
            {
                TempData["Mensaje"] = "No hay ventas de este usuario";
                return RedirectToAction("Ventas");
            }

            var nombre = ventas.First().Usuario.Nombre;
            var documento = new ReporteVentasPdf(ventas, $"Reporte de Ventas - Usuario: {nombre}");
            var pdf = documento.GeneratePdf();

            return File(pdf, "application/pdf", $"Ventas_Usuario_{usuarioId}.pdf");
        }
    }

}
