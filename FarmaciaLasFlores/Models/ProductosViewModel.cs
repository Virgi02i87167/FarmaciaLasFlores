using System.Collections.Generic;

namespace FarmaciaLasFlores.Models
{
    public class ProductosViewModel
    {
        public Productos NuevoProducto { get; set; } = new Productos(); // Producto a agregar
        public List<Productos> ListaProductos { get; set; } = new List<Productos>(); // Lista de productos existentes
    }
}
