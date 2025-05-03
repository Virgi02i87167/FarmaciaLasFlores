using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FarmaciaLasFlores.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var modelo = new UsuariosViewModel
            {
                NuevoUsuario = new Usuarios(),
                ListaUsuarios = await _context.Usuarios.Include(u => u.Rol).ToListAsync()
            };
            return View(modelo);
        }

        public async Task<IActionResult> Create()
        {
            var model = new UsuariosViewModel();

            // Filtra los roles activos
            model.ListaRoles = await _context.Roles
                .Where(r => r.Activo)
                .ToListAsync();

            // Llenar la lista de SelectListItem para el dropdown
            model.ListaRolesSelectList = model.ListaRoles.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.NombreRoles
            }).ToList();

            return View(model);
        }

        // Acción Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuariosViewModel model)
        {
            Console.WriteLine("Create action ejecutada");

            if (!ModelState.IsValid)
            {  
                try
                {
                    // Hashear la contraseña solo una vez
                    model.NuevoUsuario.Password = HashPassword(model.NuevoUsuario.Password);

                    // ConfirmPassword no se guarda en la base de datos
                    model.NuevoUsuario.ConfirmPassword = null;

                    _context.Usuarios.Add(model.NuevoUsuario);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ocurrió un error al guardar los datos: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("ModelState no válido");
            }

            //Recargar listas para que se vuelvan a mostrar correctamente
            var roles = await _context.Roles.ToListAsync();
            model.ListaRoles = roles;
            model.ListaRolesSelectList = roles.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.NombreRoles
            }).ToList();

            model.ListaUsuarios = await _context.Usuarios.ToListAsync();
            return View("Index", model);
        }

        // Acción Edit (POST)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Obtener solo los roles activos
            var roles = await _context.Roles
                .Where(r => r.Activo)
                .ToListAsync();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            var viewModel = new UsuariosViewModel
            {
                NuevoUsuario = usuario,
                ListaUsuarios = await _context.Usuarios.ToListAsync(),
                ListaRolesSelectList = roles.Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.NombreRoles
                }).ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuariosViewModel viewModel)
        {
            // Cargar la lista de roles para el formulario de edición
            viewModel.ListaRoles = await _context.Roles.ToListAsync();

            if (id != viewModel.NuevoUsuario.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    // Si el password no es nulo o vacío, hacer el hash
                    if (!string.IsNullOrEmpty(viewModel.NuevoUsuario.Password))
                    {
                        // Validar que coincidan
                        if (viewModel.NuevoUsuario.Password != viewModel.NuevoUsuario.ConfirmPassword)
                        {
                            ModelState.AddModelError("ConfirmPassword", "Las contraseñas no coinciden.");
                            return View(viewModel);
                        }

                        viewModel.NuevoUsuario.Password = HashPassword(viewModel.NuevoUsuario.Password);
                    }
                    else
                    {
                        // Mantener la contraseña anterior
                        var usuarioDB = await _context.Usuarios
                            .AsNoTracking()
                            .FirstOrDefaultAsync(u => u.Id == viewModel.NuevoUsuario.Id);

                        if (usuarioDB != null)
                        {
                            viewModel.NuevoUsuario.Password = usuarioDB.Password;
                        }
                    }

                    _context.Update(viewModel.NuevoUsuario);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Usuario actualizado exitosamente.";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(viewModel.NuevoUsuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(viewModel);
        }

        public IActionResult Delete(int id)
        {
            var usuario = _context.Usuarios
                .Include(u => u.Rol) // Si necesitas el Rol
                .FirstOrDefault(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            var viewModel = new Usuarios
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Posicion = usuario.Posicion,
                email = usuario.email,
                NombreUsuario = usuario.NombreUsuario,
                Rol = usuario.Rol,
                Estado = usuario.Estado
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Delete(Usuarios usuario)
        {
            var user = _context.Usuarios.Find(usuario.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.Estado = false; // o 0, dependiendo del tipo
            _context.SaveChanges();

            return RedirectToAction("Index");
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

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}
