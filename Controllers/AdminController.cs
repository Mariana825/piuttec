using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using piuttec.Models;

namespace piuttec.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        public AdminController(AppDbContext context) => _context = context;

        public IActionResult Index()
        {
            
            var alumnos = _context.Usuarios.Where(u => u.Rol == "Alumno").ToList();
            var docentes = _context.Usuarios.Where(u => u.Rol == "Docente").ToList();
            var materias = _context.Materias.ToList();
            var grupos = _context.Grupos.ToList();
            var usuarios = _context.Usuarios.ToList(); 

            ViewBag.Alumnos = alumnos;
            ViewBag.Docentes = docentes;
            ViewBag.Materias = materias;
            ViewBag.Grupos = grupos;
            ViewBag.Usuarios = usuarios; 

            return View();
        }


        // Registrar nuevo usuario
        [HttpPost]
        public async Task<IActionResult> RegistrarUsuario(string nombre, string correo, string rol, string contrasena)
        {
            var u = new Usuario { Nombre = nombre, Correo = correo, Rol = rol, Contrasena = contrasena };
            _context.Usuarios.Add(u);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Registrar Materia
        [HttpPost]
        public async Task<IActionResult> RegistrarMateria(string nombre)
        {
            _context.Materias.Add(new Materia { Nombre = nombre });
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Registrar Grupo
        [HttpPost]
        public async Task<IActionResult> RegistrarGrupo(string nombre)
        {
            _context.Grupos.Add(new Grupo { Nombre = nombre });
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Asignar Alumno a Grupo
        [HttpPost]
        public async Task<IActionResult> AsignarAlumnoGrupo(int alumnoId, int grupoId)
        {
            if (!_context.AlumnoGrupos.Any(ag => ag.AlumnoId == alumnoId && ag.GrupoId == grupoId))
            {
                _context.AlumnoGrupos.Add(new AlumnoGrupo { AlumnoId = alumnoId, GrupoId = grupoId });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Asignar Docente a Materia
        [HttpPost]
        public async Task<IActionResult> AsignarDocenteMateria(int docenteId, int materiaId)
        {
            if (!_context.DocenteMaterias.Any(dm => dm.DocenteId == docenteId && dm.MateriaId == materiaId))
            {
                _context.DocenteMaterias.Add(new DocenteMateria { DocenteId = docenteId, MateriaId = materiaId });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
        public IActionResult EditarUsuario(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            return View(usuario);
        }
        [HttpPost]
        public async Task<IActionResult> EditarUsuario(Usuario u)
        {
            _context.Usuarios.Update(u);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> AsignarMateriaAlumno(int alumnoId, int materiaId)
        {
            // Evitar duplicados
            var existe = _context.Calificaciones
                .Any(c => c.AlumnoId == alumnoId && c.MateriaId == materiaId);

            if (!existe)
            {
                var cal = new Calificacion
                {
                    AlumnoId = alumnoId,
                    MateriaId = materiaId,
                    DocenteId = 0, // luego se llena
                    Parcial1 = 0,
                    Parcial2 = 0,
                    Parcial3 = 0,
                    Final = 0
                };

                _context.Calificaciones.Add(cal);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}