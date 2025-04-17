using System.ComponentModel.DataAnnotations;
using iTextSharp.text;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmaciaLasFlores.Models
{
    public class Usuarios
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe agregar un nombre.")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Debe agregar una posicion.")]
        public string Posicion { get; set; }

        [Required(ErrorMessage = "Debe agregar un email.")]
        [StringLength(50)]
        public string email { get; set; }

        [Required(ErrorMessage = "Debe agregar un nombre de usuario.")]
        [StringLength(50)]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "Debe agregar una contraseña.")]
        public string Password { get; set; }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public int RolId { get; set; }

        public Roles Rol { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estado .")]
        public bool Estado { get; set; } = true;
    }
}
