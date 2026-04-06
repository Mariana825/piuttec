using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using piuttec.Models;
using System.Linq;
using System.Threading.Tasks;

namespace piuttec.Controllers
{
    public class AlumnoController : Controller
    {
        private readonly AppDbContext _context;
        public AlumnoController(AppDbContext context) => _context = context;

        // Vista principal del alumno
        public async Task<IActionResult> Index(int id)
        {
            // Obtener alumno
            var alumno = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id && u.Rol == "Alumno");

            if (alumno == null)
                return NotFound("Alumno no encontrado");

            // Obtener TODOS los grupos del alumno (por si tiene más de uno)
            var grupos = await _context.AlumnoGrupos
                .Include(ag => ag.Grupo)
                .Where(ag => ag.AlumnoId == id)
                .Select(ag => ag.Grupo.Nombre)
                .ToListAsync();

            // Convertir a string (ej: "1A, 2B")
            var grupoTexto = grupos.Any() ? string.Join(", ", grupos) : "Sin grupo";

            // Obtener calificaciones con materias
            var calificaciones = await _context.Calificaciones
                .Include(c => c.Materia)
                .Where(c => c.AlumnoId == id)
                .ToListAsync();

            // Evitar null en materias
            var materias = calificaciones
                .Where(c => c.Materia != null)
                .Select(c => c.Materia.Nombre)
                .ToList();

            // ViewModel
            var viewModel = new AlumnoViewModel
            {
                Id = alumno.Id,
                Nombre = alumno.Nombre,
                Grupo = grupoTexto,
                Materias = materias,
                Calificaciones = calificaciones
            };

            return View(viewModel);
        }
    }
}