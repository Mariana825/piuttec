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

        // Constructor: recibe el contexto de la base de datos
        public AlumnoController(AppDbContext context) => _context = context;

        // Vista principal del alumno
        // YA NO RECIBE id → evitamos manipulación en la URL
        public async Task<IActionResult> Index()
        {
            // Obtener el ID del usuario desde la sesión
            var userId = HttpContext.Session.GetInt32("UserId");

            // Obtener el rol desde la sesión
            var rol = HttpContext.Session.GetString("Rol");

            // Validación de seguridad:
            // - Si no hay sesión → redirige al login
            // - Si no es alumno → no tiene acceso
            if (userId == null || rol != "Alumno")
                return RedirectToAction("Index", "Login");

            // Convertimos el valor nullable a int normal
            int id = userId.Value;

            // Obtener alumno SOLO con el ID de sesión
            var alumno = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id && u.Rol == "Alumno");

            // Si no existe el alumno → error
            if (alumno == null)
                return NotFound("Alumno no encontrado");

            // Obtener TODOS los grupos del alumno
            var grupos = await _context.AlumnoGrupos
                .Include(ag => ag.Grupo)
                .Where(ag => ag.AlumnoId == id)
                .Select(ag => ag.Grupo.Nombre)
                .ToListAsync();

            // Convertir a texto
            var grupoTexto = grupos.Any() ? string.Join(", ", grupos) : "Sin grupo";

            // Obtener calificaciones con materias
            var calificaciones = await _context.Calificaciones
                .Include(c => c.Materia)
                .Where(c => c.AlumnoId == id)
                .ToListAsync();

            // Evitar null en materias (seguridad y estabilidad)
            var materias = calificaciones
                .Where(c => c.Materia != null)
                .Select(c => c.Materia.Nombre)
                .ToList();

            // Crear ViewModel
            var viewModel = new AlumnoViewModel
            {
                Id = alumno.Id,
                Nombre = alumno.Nombre,
                Grupo = grupoTexto,
                Materias = materias,
                Calificaciones = calificaciones
            };

            // Retornar la vista con los datos
            return View(viewModel);
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // elimina sesión
            return Redirect("/Login");   // redirige directo
        }
    }
}