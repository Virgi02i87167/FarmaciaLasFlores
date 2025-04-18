using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using FarmaciaLasFlores.Db;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace FarmaciaLasFlores.Controllers
{
    public class ReporteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReporteController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GenerarPDF(DateTime? FechaInicio, DateTime? FechaFin)
        {
            // Obtener la lista de productos con filtros solo por fechas
            var productos = _context.Productos.AsQueryable();

            // Filtrado por fecha de registro
            if (FechaInicio.HasValue)
            {
                productos = productos.Where(p => p.FechaRegistro >= FechaInicio.Value);
            }

            if (FechaFin.HasValue)
            {
                productos = productos.Where(p => p.FechaRegistro <= FechaFin.Value);
            }

            var listaProductos = productos.ToList();

            // Crear un documento PDF en memoria
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Agregar título
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Reporte de Productos", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(title);
                document.Add(new Paragraph("\n"));

                // Crear la tabla PDF
                PdfPTable table = new PdfPTable(7); // 7 columnas
                table.WidthPercentage = 100;

                // Encabezados de la tabla
                string[] headers = { "Nombre", "Cantidad", "Precio", "Lote", "Fecha Registro", "Fecha Vencimiento", "Tipo" };
                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)))
                    {
                        BackgroundColor = BaseColor.LightGray,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }

                // Agregar datos a la tabla
                foreach (var producto in listaProductos)
                {
                    table.AddCell(new PdfPCell(new Phrase(producto.Nombre)));
                    table.AddCell(new PdfPCell(new Phrase(producto.Cantidad.ToString())));
                    table.AddCell(new PdfPCell(new Phrase(producto.PrecioCompra.ToString("C"))));
                    table.AddCell(new PdfPCell(new Phrase(producto.Lote)));
                    table.AddCell(new PdfPCell(new Phrase(producto.FechaRegistro.ToString("yyyy-MM-dd"))));
                    table.AddCell(new PdfPCell(new Phrase(producto.FechaVencimiento.ToString("yyyy-MM-dd"))));
                    table.AddCell(new PdfPCell(new Phrase(producto.MedicamentosId)));
                }

                document.Add(table);
                document.Close();
                writer.Close();

                // Reiniciar la posición del stream antes de devolverlo
                memoryStream.Position = 0;

                return File(memoryStream.ToArray(), "application/pdf", "Reporte_Productos.pdf");
            }
        }
    }
}
