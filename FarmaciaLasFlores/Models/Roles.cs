using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FarmaciaLasFlores.Models
{
    public class Roles
    {
        [Key]
        public int Id { get; set; }

        [Required]
        //validacion de longitud
        [StringLength(50, ErrorMessage = "El nombre del rol no puede exceder los 50 caracteres.")]
        public string NombreRoles { get; set; }
    }
}