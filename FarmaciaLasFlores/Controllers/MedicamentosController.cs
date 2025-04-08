using FarmaciaLasFlores.Db;
using FarmaciaLasFlores.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaLasFlores.Controllers
{
    public class MedicamentosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicamentosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Mostrar lista de TipossMedicamentos con búsqueda
        public async Task<IActionResult> Index(string searchString)
        {

            var medicamentos = await _context.Medicamentos
        .Include(m => m.Productos)
        .ToListAsync();

            var viewModel = new MedicamentosViewModel
            {
                ListaMedicamentos = medicamentos,
                NuevoMedicamento = new Medicamentos() // Objeto vacío para el formulario
            };

            return View(viewModel);
        }

        // GET: Medicamentos/Create
        public IActionResult Create()
        {
            return View();
        }


        // Guardar nuevo producto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicamentosViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.ListaMedicamentos = await _context.Medicamentos.ToListAsync();
                return View("Index", viewModel);
            }

            // Validaciones adicionales
            if (string.IsNullOrWhiteSpace(viewModel.NuevoMedicamento.TipoMedicamento))
            {
                ModelState.AddModelError("NuevoMedicamento.TipoMedicamento", "El Tipo medicmaneton no puede estar vacío.");
            }

            // Si hay errores, devolvemos la vista con los mensajes
            if (!ModelState.IsValid)
            {
                viewModel.ListaMedicamentos = await _context.Medicamentos.ToListAsync();
                return View("Index", viewModel);
            }

            try
            {
                _context.Medicamentos.Add(viewModel.NuevoMedicamento);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar el Tipo Medicamento: {ex.Message}");
            }

            return RedirectToAction("Index");
        }
    }
}
