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
        public DateTime FechaVenta { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Productos Producto { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; }


        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

    }
}

