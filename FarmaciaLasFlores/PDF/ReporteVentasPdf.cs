using FarmaciaLasFlores.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;



public class ReporteVentasPdf : IDocument
{
    private readonly List<Ventas> ventas;
    private readonly string titulo;

    public ReporteVentasPdf(List<Ventas> ventas, string titulo)
    {
        this.ventas = ventas;
        this.titulo = titulo;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);
            page.Header().Text(titulo).FontSize(18).Bold().AlignCenter();
            page.Content().Column(col =>
            {
                foreach (var venta in ventas)
                {
                    col.Item().Text($"Venta #{venta.Id} - {venta.FechaVenta:dd/MM/yyyy} - {venta.Usuario.Nombre}").Bold();
                    col.Item().Element(c => CrearTablaDetalles(c, venta));
                    col.Item().PaddingBottom(20).Element(e =>
                        e.Text($"Total: ${venta.Total:F2}")
                        .FontSize(12)
                        .Bold()
                        .AlignRight()
                    );

                    col.Item().PaddingVertical(10).Element(e =>
                        e.LineHorizontal(0.5f)
                        .LineColor(Colors.Grey.Lighten2)
                    );
                }
            });
        });
    }

    void CrearTablaDetalles(IContainer container, Ventas venta)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.ConstantColumn(60);
                columns.ConstantColumn(80);
                columns.ConstantColumn(80);
            });

            table.Header(header =>
            {
                header.Cell().Text("Producto").Bold();
                header.Cell().Text("Cant.").Bold();
                header.Cell().Text("Precio").Bold();
                header.Cell().Text("Subtotal").Bold();
            });

            foreach (var detalle in venta.Detalles)
            {
                table.Cell().Text(detalle.Producto.Nombre);
                table.Cell().Text(detalle.Cantidad.ToString());
                table.Cell().Text($"${detalle.PrecioVenta:F2}");
                table.Cell().Text($"${detalle.Subtotal:F2}");
            }
        });
    }
}
