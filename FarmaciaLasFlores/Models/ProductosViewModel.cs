using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace FarmaciaLasFlores.Models
{
    public class ProductosViewModel
    {
        public Productos NuevoProducto { get; set; } = new Productos(); // Producto a agregar
        public List<Productos> ListaProductos { get; set; } = new List<Productos>(); // Lista de productos existentes

        public string SearchString { get; set; } = string.Empty; //para buscar productos

        public int? TipoMedicamentoId { get; set; }
        public List<SelectListItem> ListaTiposMedicamento { get; set; } = new List<SelectListItem>();
    }
}