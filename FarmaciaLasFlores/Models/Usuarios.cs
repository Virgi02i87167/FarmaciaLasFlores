using System.ComponentModel.DataAnnotations;

namespace FarmaciaLasFlores.Models
{
    public class Usuarios
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string gmail { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
