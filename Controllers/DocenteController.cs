using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using piuttec.Models;
using System.Linq;
using System.Threading.Tasks;

namespace piuttec.Controllers
{
    public class DocenteController : Controller
    {
        private readonly AppDbContext _context;

        // Constructor: inyección del contexto de base de datos
        public DocenteController(AppDbContext context) => _context = context;

        // YA NO RECIBE ID → se obtiene desde sesión
        public IActionResult Index()
        {
            // Obtener datos de sesión
            var userId = HttpContext.Session.GetInt32("UserId");
            var rol = HttpContext.Session.GetString("Rol");

            // Validación de acceso
            if (userId == null || rol != "Docente")
                return RedirectToAction("Index", "Login");

            int id = userId.Value;

            // Buscar docente autenticado
            var docente = _context.Usuarios.Find(id);
            if (docente == null) return NotFound();

            // Materias del docente
            var materias = _context.DocenteMaterias
                .Include(dm => dm.Materia)
                .Where(dm => dm.DocenteId == id)
                .Select(dm => dm.Materia)
                .ToList();

            // Alumnos por materia
            var alumnosPorMateria = _context.Calificaciones
                .Include(c => c.Alumno)
                .Include(c => c.Materia)
                .Where(c => materias.Select(m => m.Id).Contains(c.MateriaId))
                .ToList();

            // Enviar datos a la vista
            ViewBag.Materias = materias;
            ViewBag.AlumnosPorMateria = alumnosPorMateria;

            var model = new DocenteViewModel
            {
                Nombre = docente.Nombre,
                Materias = materias.Select(m => m.Nombre).ToList()
            };

            return View(model);
        }

        // Registrar o actualizar calificación
        [HttpPost]
        public async Task<IActionResult> RegistrarCalificacion(
            int AlumnoId, int MateriaId,
            double? Parcial1, double? Parcial2, double? Parcial3, double? Final)
        {
            // Obtener docente REAL desde sesión
            var userId = HttpContext.Session.GetInt32("UserId");
            var rol = HttpContext.Session.GetString("Rol");

            // Validación de seguridad
            if (userId == null || rol != "Docente")
                return RedirectToAction("Index", "Login");

            int docenteId = userId.Value;

            // Validar que el docente imparte esa materia
            var docenteMateria = await _context.DocenteMaterias
                .AnyAsync(dm => dm.DocenteId == docenteId && dm.MateriaId == MateriaId);

            if (!docenteMateria)
                return RedirectToAction("Index");

            // Buscar si ya existe calificación
            var cal = await _context.Calificaciones
                .FirstOrDefaultAsync(c => c.AlumnoId == AlumnoId && c.MateriaId == MateriaId);

            if (cal == null)
            {
                // Crear nueva calificación
                cal = new Calificacion
                {
                    AlumnoId = AlumnoId,
                    MateriaId = MateriaId,
                    DocenteId = docenteId,

                    // Si no mandan valor → se queda en 0 (por defecto)
                    Parcial1 = Parcial1 ?? 0,
                    Parcial2 = Parcial2 ?? 0,
                    Parcial3 = Parcial3 ?? 0,
                    Final = Final ?? 0
                };

                _context.Calificaciones.Add(cal);
            }
            else
            {
                // 🔥 SOLO actualizar si el valor viene (NO sobreescribe con 0)
                if (Parcial1.HasValue)
                    cal.Parcial1 = Parcial1.Value;

                if (Parcial2.HasValue)
                    cal.Parcial2 = Parcial2.Value;

                if (Parcial3.HasValue)
                    cal.Parcial3 = Parcial3.Value;

                if (Final.HasValue)
                    cal.Final = Final.Value;

                // Refuerza que el docente correcto sea el que modifica
                cal.DocenteId = docenteId;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // 🔐 Cerrar sesión
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // elimina sesión
            return Redirect("/Login");
        }
    }
}