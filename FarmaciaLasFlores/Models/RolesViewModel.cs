namespace FarmaciaLasFlores.Models
{
    public class RolesViewModel
    {
        public Roles NuevoRol { get; set; }  // Para la creación de un nuevo rol
        public List<Roles> ListaRoles { get; set; } = new List<Roles>();// Solo para mostrar los roles existentes
        
    }
}
