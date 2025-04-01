namespace FarmaciaLasFlores.Models
{
    public class VentasViewModel
    {
        public Ventas NuevaVenta { get; set; } = new Ventas(); // Venta actual que se está registrando
        public List<Productos> ListaProductos { get; set; } = new List<Productos>(); // Productos disponibles
        public List<Ventas> ListaVentas { get; set; } = new List<Ventas>(); // Lista de ventas realizadas
    }
}

