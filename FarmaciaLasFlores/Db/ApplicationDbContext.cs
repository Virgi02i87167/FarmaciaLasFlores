using FarmaciaLasFlores.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaLasFlores.Db
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Productos> Productos { get; set; }
        public DbSet<Ventas> Ventas { get; set; }

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
            modelBuilder.Entity<Ventas>()
                .Property(p => p.PrecioVenta)
                .HasColumnType("decimal(18,2)");  // Especificamos precisión y escala para 'Precio'
        }
    }
}
