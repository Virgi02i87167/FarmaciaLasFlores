using System.ComponentModel.DataAnnotations;

namespace FarmaciaLasFlores.Models
{
    public class Medicamentos
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TipoMedicamento { get; set; }

        public List<Productos> Productos { get; set; } = new List<Productos>();
    }
}
