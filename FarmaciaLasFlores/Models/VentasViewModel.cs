namespace FarmaciaLasFlores.Models
{
    public class VentasViewModel
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public Ventas NuevaVenta { get; set; } = new Ventas(); // Venta actual que se está registrando
        public List<Productos> ListaProductos { get; set; } = new List<Productos>(); // Productos disponibles
        public List<Ventas> ListaVentas { get; set; } = new List<Ventas>(); // Lista de ventas realizadas

        public List<ItemCarrito> ListaCarrito { get; set; } = new List<ItemCarrito>();

        public List<Usuarios> ListaUsuarios { get; set; }
    }

    public class EditarVentaViewModel
    {
        public int VentaId { get; set; }

        public List<DetalleVentaViewModel> Detalles { get; set; }

        public decimal Total => Detalles?.Sum(d => d.Subtotal) ?? 0;
    }

    public class DetalleVentaViewModel
    {
        public int DetalleId { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal Subtotal => Cantidad * PrecioVenta;
    }
}

