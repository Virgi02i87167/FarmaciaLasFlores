using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaLasFlores.Servicios
{
    public class VentasService
    {
        private readonly ApplicationDbContext _context;

        public VentasService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Ventas ObtenerVentaConDetalles(int id)
        {
            return _context.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefault(v => v.Id == id);
        }

        public List<Ventas> ObtenerVentasPorMes(int mes, int anio)
        {
            return _context.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(v => v.FechaVenta.Month == mes && v.FechaVenta.Year == anio)
                .ToList();
        }

        public List<Ventas> ObtenerVentasPorUsuario(int usuarioId)
        {
            return _context.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(v => v.UsuarioId == usuarioId)
                .ToList();
        }
    }
}
