namespace FarmaciaLasFlores.Models
{
    public class RolePermissionViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }

        public List<PermissionViewModel> Permissions { get; set; } = new();
    }
}
