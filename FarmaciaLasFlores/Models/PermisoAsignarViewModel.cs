namespace FarmaciaLasFlores.Models
{

    public class PermisoAsignarViewModel
    {
        public int RolId { get; set; }
        public List<PermisoSeleccionado> Permisos { get; set; }
    }

    public class PermisoSeleccionado
    {
        public string PermisoId { get; set; }  // Usamos string porque los nombres pueden ser "Ventas", "Reportes", etc.
        public string Nombre { get; set; }
        public bool Seleccionado { get; set; }
    }
}
