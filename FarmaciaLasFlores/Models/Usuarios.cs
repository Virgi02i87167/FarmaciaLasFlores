using System.ComponentModel.DataAnnotations;
using iTextSharp.text;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmaciaLasFlores.Models
{
    public class Usuarios
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(50)]
        public string Posicion { get; set; }

        [Required]
        [StringLength(50)]
        public string email { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; }

        [Required]
        public string Password { get; set; }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        //Llave foranea tabla Roles

        public int RolId { get; set; }

        [ForeignKey("RolId")]
        public Roles Rol { get; set; }
    }
}
