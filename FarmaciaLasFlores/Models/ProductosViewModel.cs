using System.Collections.Generic;

namespace FarmaciaLasFlores.Models
{
    public class ProductosViewModel
    {
        public Productos NuevoProducto { get; set; } = new Productos(); // Producto a agregar
        public List<Productos> ListaProductos { get; set; } = new List<Productos>(); // Lista de productos existentes

        public string SearchString { get; set; } = string.Empty; //para buscar productos

        // Nuevas propiedades para el filtrado por fechas
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}