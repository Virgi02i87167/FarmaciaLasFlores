namespace FarmaciaLasFlores.Models
{
    public class MedicamentosViewModel
    {
        public Medicamentos NuevoMedicamento { get; set; }
        public List<Medicamentos> ListaMedicamentos { get; set; }

        public MedicamentosViewModel()
        {
            NuevoMedicamento = new Medicamentos(); // Producto a agregar
            ListaMedicamentos = new List<Medicamentos>(); // Lista de productos existentes
        }
    }
}
