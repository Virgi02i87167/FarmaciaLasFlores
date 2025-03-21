using FarmaciaLasFlores.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaLasFlores.Db
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Usuarios> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuarios>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();

            modelBuilder.Entity<Usuarios>()
                .HasIndex(u => u.email)
                .IsUnique();
        }

    }
}