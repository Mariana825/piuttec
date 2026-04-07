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

            //Validación de acceso
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

            // Alumnos por materia (desde calificaciones)
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
            double Parcial1, double Parcial2, double Parcial3, double Final)
        {
            // Obtener docente REAL desde sesión (NO confiar en el formulario)
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
                return RedirectToAction("Index"); // evita manipulación

            // Validar que el alumno pertenece a esa materia
            var alumnoMateria = await _context.Calificaciones
                .AnyAsync(c => c.AlumnoId == AlumnoId && c.MateriaId == MateriaId);

            if (!alumnoMateria)
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
                    DocenteId = docenteId, // 🔐 SIEMPRE desde sesión
                    Parcial1 = Parcial1,
                    Parcial2 = Parcial2,
                    Parcial3 = Parcial3,
                    Final = Final
                };

                _context.Calificaciones.Add(cal);
            }
            else
            {
                // Actualizar existente
                cal.Parcial1 = Parcial1;
                cal.Parcial2 = Parcial2;
                cal.Parcial3 = Parcial3;
                cal.Final = Final;

                //Refuerza que el docente correcto es el que modifica
                cal.DocenteId = docenteId;
            }

            await _context.SaveChangesAsync();

            // Redirección segura (sin ID en URL)
            return RedirectToAction("Index");
        }
    }
}