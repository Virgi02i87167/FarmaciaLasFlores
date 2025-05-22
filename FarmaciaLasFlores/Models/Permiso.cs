using FarmaciaLasFlores.Models;

public class Permiso
{
    public int Id { get; set; }
    public int RolId { get; set; }
    public string Nombre { get; set; }

    public Roles Rol { get; set; }
}
