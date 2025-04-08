using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmaciaLasFlores.Models
{
    public class Ventas
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime FechaVenta { get; set; } = DateTime.Now;

        [Required]
        //Llave foranea tabla productos
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Productos Producto { get; set; }

        [Required]
        //Llave foranea tabla Usuario
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuarios Usuario { get; set; } 

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
    }
}

