using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using piuttec.Models;

namespace piuttec.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        // Inyección del contexto de base de datos
        public LoginController(AppDbContext context) => _context = context;

        // GET: /Login
        [HttpGet]
        public IActionResult Index()
        {
            // Muestra la vista del login
            return View();
        }

        // POST: /Login
        [HttpPost]
        public async Task<IActionResult> Index(string correo, string contrasena)
        {
            
            // Evita que se envíen datos vacíos o nulos (seguridad básica y evita errores)
            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
            {
                ViewBag.Error = "Todos los campos son obligatorios";
                return View();
            }

            //Se aplica el HASH
            // Buscar usuario solo por correo
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correo);

           
            // Si el usuario no existe, se agrega un pequeño retraso
            // Esto evita ataques donde intentan descubrir correos válidos
            if (usuario == null)
            {
                await Task.Delay(500); // simula tiempo de procesamiento
                ViewBag.Error = "Correo o contraseña incorrectos";
                return View();
            }

            // VERIFICACIÓN DE CONTRASEÑA CON BCRYPT
            // Compara la contraseña ingresada con el hash guardado en la BD
            if (!BCrypt.Net.BCrypt.Verify(contrasena, usuario.Contrasena))
            {
                ViewBag.Error = "Correo o contraseña incorrectos";
                return View();
            }

            // LIMPIAR SESIÓN ANTERIOR
            // Evita problemas de seguridad si ya había una sesión activa
            HttpContext.Session.Clear();

            //Se guardan las sesiones
            // Guardar datos del usuario en sesión para mantenerlo autenticado
            HttpContext.Session.SetInt32("UserId", usuario.Id); // Guarda ID del usuario
            HttpContext.Session.SetString("Rol", usuario.Rol);  // Guarda rol (Admin, Alumno, Docente)

            // Redirige según rol
            return usuario.Rol switch
            {
                // Redirección controlada según permisos
                "Admin" => RedirectToAction("Index", "Admin"),
                "Alumno" => RedirectToAction("Index", "Alumno"),
                "Docente" => RedirectToAction("Index", "Docente"),
                _ => View() // fallback por si algo falla
            };
        }
    }
}