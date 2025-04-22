namespace FarmaciaLasFlores.Models
{
    public class ItemCarrito
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Cantidad { get; set; } = 1;
        public decimal Subtotal => PrecioVenta * Cantidad;
    }
}
