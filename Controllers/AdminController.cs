using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using piuttec.Models;

namespace piuttec.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        public AdminController(AppDbContext context) => _context = context;

        // Método reutilizable para validar Admin
        private bool EsAdmin()
        {
            return HttpContext.Session.GetString("Rol") == "Admin";
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // elimina sesión
            return Redirect("/Login");   // redirige directo
        }

        public IActionResult Index()
        {
            if (!EsAdmin())
                return RedirectToAction("Index", "Login");

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
            if (!EsAdmin())
                return Unauthorized();

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
                return BadRequest("Datos inválidos");

            var hash = BCrypt.Net.BCrypt.HashPassword(contrasena);

            var u = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                Rol = rol,
                Contrasena = hash
            };

            _context.Usuarios.Add(u);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Registrar Materia
        [HttpPost]
        public async Task<IActionResult> RegistrarMateria(string nombre)
        {
            if (!EsAdmin())
                return Unauthorized();

            if (string.IsNullOrEmpty(nombre))
                return BadRequest("Nombre inválido");

            _context.Materias.Add(new Materia { Nombre = nombre });
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Registrar Grupo
        [HttpPost]
        public async Task<IActionResult> RegistrarGrupo(string nombre)
        {
            if (!EsAdmin())
                return Unauthorized();

            if (string.IsNullOrEmpty(nombre))
                return BadRequest("Nombre inválido");

            _context.Grupos.Add(new Grupo { Nombre = nombre });
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Asignar Alumno a Grupo
        [HttpPost]
        public async Task<IActionResult> AsignarAlumnoGrupo(int alumnoId, int grupoId)
        {
            if (!EsAdmin())
                return Unauthorized();

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
            if (!EsAdmin())
                return Unauthorized();

            if (!_context.DocenteMaterias.Any(dm => dm.DocenteId == docenteId && dm.MateriaId == materiaId))
            {
                _context.DocenteMaterias.Add(new DocenteMateria { DocenteId = docenteId, MateriaId = materiaId });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // PROTEGIDO para que no se pueda eliminar en URL
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            if (!EsAdmin())
                return Unauthorized();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Editar usuario (GET)
        public IActionResult EditarUsuario(int id)
        {
            if (!EsAdmin())
                return Unauthorized();

            var usuario = _context.Usuarios.Find(id);
            return View(usuario);
        }

        // Editar usuario (POST)
        [HttpPost]
        public async Task<IActionResult> EditarUsuario(Usuario u)
        {
            if (!EsAdmin())
                return Unauthorized();

            _context.Usuarios.Update(u);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Asignar materia a alumno
        [HttpPost]
        public async Task<IActionResult> AsignarMateriaAlumno(int alumnoId, int materiaId)
        {
            if (!EsAdmin())
                return Unauthorized();

            var existe = _context.Calificaciones
                .Any(c => c.AlumnoId == alumnoId && c.MateriaId == materiaId);

            if (!existe)
            {
                var cal = new Calificacion
                {
                    AlumnoId = alumnoId,
                    MateriaId = materiaId,
                    DocenteId = 0,
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