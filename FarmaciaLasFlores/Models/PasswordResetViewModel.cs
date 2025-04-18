using System.ComponentModel.DataAnnotations;

namespace FarmaciaLasFlores.Models
{
    public class PasswordResetViewModel
    {
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }
        }
}
