namespace FarmaciaLasFlores.Models
{
    public class UsuariosViewModel
    {
        public Usuarios NuevoUsuario { get; set; }
        public List<Usuarios> ListaUsuarios { get; set; }

        public UsuariosViewModel()
        {
            NuevoUsuario = new Usuarios();
            ListaUsuarios = new List<Usuarios>();
        }
    }
}
