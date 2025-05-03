using System.ComponentModel.DataAnnotations;

namespace FarmaciaLasFlores.Models
{
    public class Medicamentos
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe agregar un TipoMedicamento.")]
        [StringLength(100)]
        public string TipoMedicamento { get; set; }

        public List<Productos> Productos { get; set; } = new List<Productos>();

        [Required(ErrorMessage = "Debe seleccionar un estado .")]
        public bool Estado { get; set; } = true;
    }
}
