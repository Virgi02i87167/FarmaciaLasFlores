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
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuarios Usuario { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; } // Suma de todos los subtotales

        [Required]
        public bool Estado  { get; set; } = true;

        // Relación con los detalles de venta
        public ICollection<DetalleVenta> Detalles { get; set; }
    }
}

