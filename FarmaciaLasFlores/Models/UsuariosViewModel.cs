using Microsoft.AspNetCore.Mvc.Rendering;

namespace FarmaciaLasFlores.Models
{
    public class UsuariosViewModel
    {
        public Usuarios NuevoUsuario { get; set; }
        public List<Usuarios> ListaUsuarios { get; set; }
        public List<Roles> ListaRoles { get; set; } // Solo para mostrar los roles existentes


        public List<SelectListItem> ListaRolesSelectList { get; set; }

        public UsuariosViewModel()
        {
            NuevoUsuario = new Usuarios();
            ListaUsuarios = new List<Usuarios>();

            ListaRoles = new List<Roles>();// Solo para mostrar los roles existentes

            ListaRolesSelectList = new List<SelectListItem>();
        }
    }
}
