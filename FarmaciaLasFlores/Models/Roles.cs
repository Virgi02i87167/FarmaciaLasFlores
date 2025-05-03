using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FarmaciaLasFlores.Models
{
    public class Roles
    {
        [Key]
        public int Id { get; set; }

        //validacion de longitud
        [Required(ErrorMessage = "Debe agregar un rol.")]
        [StringLength(50, ErrorMessage = "El nombre del rol no puede exceder los 50 caracteres.")]
        public string NombreRoles { get; set; }
        public bool Activo { get; set; } = true; // Estado de rol activo o inactivo
    }
}