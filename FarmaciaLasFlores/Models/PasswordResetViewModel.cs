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
    }
}
