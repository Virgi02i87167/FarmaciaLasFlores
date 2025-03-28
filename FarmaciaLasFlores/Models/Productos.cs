using System;
using System.ComponentModel.DataAnnotations;

namespace FarmaciaLasFlores.Models
{
    // Entidad que corresponde a la tabla productos
    public class Productos
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Debe ingresar una cantidad válida.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "Debe ingresar un precio válido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El lote del producto es obligatorio.")]
        [StringLength(50, ErrorMessage = "El lote no puede superar los 50 caracteres.")]
        public string Lote { get; set; }

        [Required(ErrorMessage = "La fecha de registro es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaRegistro { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaVencimiento { get; set; }

        [Required(ErrorMessage = "El tipo de producto es obligatorio.")]
        [StringLength(50, ErrorMessage = "El tipo de producto no puede superar los 50 caracteres.")]
        public string TipoMedicamento { get; set; }
    }
}