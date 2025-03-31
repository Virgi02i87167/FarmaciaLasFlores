using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace FarmaciaLasFlores.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Vista para solicitar recuperación de contraseña
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(PasswordResetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Usuarios.FirstOrDefault(u => u.email == model.Email);
            if (user != null)
            {
                string token = GenerateToken();
                user.ResetToken = token;
                user.ResetTokenExpiry = DateTime.Now.AddHours(1);
                _context.SaveChanges();

                bool emailSent = SendResetEmail(user.email, token);
                ViewBag.Message = emailSent
                    ? "Se ha enviado un enlace de recuperación a su correo."
                    : "No se pudo enviar el correo. Verifique su dirección.";
            }
            else
            {
                ViewBag.Message = "Correo no encontrado.";
            }

            return View();
        }

        public IActionResult ResetPassword(string token)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.Now);
            if (user == null)
            {
                return NotFound("El enlace de recuperación ha expirado o es inválido.");
            }

            ViewData["Token"] = token; // Pasar el token a la vista
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Usuarios.FirstOrDefault(u => u.ResetToken == model.Token && u.ResetTokenExpiry > DateTime.Now);
            if (user != null)
            {
                user.Password = HashPassword(model.NewPassword);
                user.ResetToken = null;
                user.ResetTokenExpiry = null;

                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    // Captura y muestra los detalles de la excepción interna
                    Console.WriteLine(ex.InnerException?.Message);
                }


                //_context.SaveChanges();
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Message = "El enlace es inválido o ha expirado.";
            return View();
        }

        private string GenerateToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }

        private bool SendResetEmail(string email, string token)
        {
            string resetLink = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme);
            string body = $"<p>Haga clic en el siguiente enlace para restablecer su contraseña:</p>" +
                          $"<p><a href='{resetLink}'>Restablecer Contraseña</a></p>";

            try
            {
                using (var smtp = new SmtpClient("smtp.gmail.com"))
                {
                    smtp.Port = 587;  // Usa 587 con TLS (más seguro)
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential("hvirgilio088@gmail.com", "xlye fguy mfib vxyo");

                    var message = new MailMessage
                    {
                        From = new MailAddress("hvirgilio088@gmail.com", "Farmacia Las Flores"),
                        Subject = "Restablecer contraseña",
                        Body = body,
                        IsBodyHtml = true
                    };

                    message.To.Add(email);

                    smtp.Send(message);
                    Console.WriteLine("Correo enviado correctamente a: " + email);
                    return true;
                }
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"SMTP Error: {smtpEx.Message}");
                Console.WriteLine(smtpEx.InnerException?.Message);
                Console.WriteLine(smtpEx.StackTrace);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
                Console.WriteLine(ex.InnerException?.Message);
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
