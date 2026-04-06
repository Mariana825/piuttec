using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using piuttec.Models;

namespace piuttec.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;
        public LoginController(AppDbContext context) => _context = context;

        // GET: /Login
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Login
        [HttpPost]
        public async Task<IActionResult> Index(string correo, string contrasena)
        {
            var usuario = await _context.Usuarios
     .FirstOrDefaultAsync(u => u.Correo == correo && u.Contrasena == contrasena);
            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos";
                return View();
            }

            // Redirige según rol
            return usuario.Rol switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Alumno" => RedirectToAction("Index", "Alumno", new { id = usuario.Id }),
                "Docente" => RedirectToAction("Index", "Docente", new { id = usuario.Id }),
                _ => View()
            };
        }
    }
}