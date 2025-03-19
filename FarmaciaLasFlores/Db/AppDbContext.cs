using FarmaciaLasFlores.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaLasFlores.Db
{
    //Esta es la clase que maneja la conexion con la BD
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Usuarios> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}