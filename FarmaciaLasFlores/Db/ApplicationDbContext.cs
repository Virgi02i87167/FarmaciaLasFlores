using FarmaciaLasFlores.Models;
using FarmaciaLasFlores.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaLasFlores.Db
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Productos> Productos { get; set; }
        public DbSet<Ventas> Ventas { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Medicamentos> Medicamentos {  get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public object Permissions { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración para Usuarios
            modelBuilder.Entity<Usuarios>()
                .HasIndex(u => u.NombreUsuario)
                .IsUnique();

            modelBuilder.Entity<Usuarios>()
                .HasIndex(u => u.email)
                .IsUnique();

            // Configuración para Productos
            modelBuilder.Entity<DetalleVenta>()
                .Property(p => p.PrecioVenta)
                .HasColumnType("decimal(18,2)");  // Especificamos precisión y escala para 'Precio'


            //Datos roles del sistema
            modelBuilder.Entity<Roles>().HasData(
                new Roles { Id = 1, NombreRoles = RolesSistema.Administrador, Activo = true },
                new Roles { Id = 2, NombreRoles = RolesSistema.Vendedor, Activo = true },
                new Roles { Id = 3, NombreRoles = RolesSistema.Supervisor, Activo = true }
            );

        }
    }
}
