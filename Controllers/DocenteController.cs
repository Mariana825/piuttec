using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using piuttec.Models;
using System;

namespace piuttec.Controllers
{
    
    public class DocenteController : Controller
    {
        private readonly AppDbContext _context;
        public DocenteController(AppDbContext context) => _context = context;

        public IActionResult Index(int id)
        {
            var docente = _context.Usuarios.Find(id);
            if (docente == null) return NotFound();

            // Materias del docente
            var materias = _context.DocenteMaterias
                .Include(dm => dm.Materia)
                .Where(dm => dm.DocenteId == id)
                .Select(dm => dm.Materia)
                .ToList();


            // 🔥 CLAVE: alumnos por materia usando Calificaciones
            var alumnosPorMateria = _context.Calificaciones
                .Include(c => c.Alumno)
                .Include(c => c.Materia)
                .Where(c => materias.Select(m => m.Id).Contains(c.MateriaId))
                .ToList(); ;


            ViewBag.Materias = materias;
            ViewBag.AlumnosPorMateria = alumnosPorMateria;
            ViewBag.DocenteId = id;

            var model = new DocenteViewModel
            {
                Nombre = docente.Nombre,
                Materias = materias.Select(m => m.Nombre).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarCalificacion(
    int AlumnoId, int MateriaId, int DocenteId,
    double Parcial1, double Parcial2, double Parcial3, double Final)
        {
           
            var docenteMateria = await _context.DocenteMaterias
                .AnyAsync(dm => dm.DocenteId == DocenteId && dm.MateriaId == MateriaId);

            if (!docenteMateria)
                return RedirectToAction("Index", new { id = DocenteId }); // simple, sin romper flujo

            
            var alumnoMateria = await _context.Calificaciones
                .AnyAsync(c => c.AlumnoId == AlumnoId && c.MateriaId == MateriaId);

            if (!alumnoMateria)
                return RedirectToAction("Index", new { id = DocenteId });

            
            var cal = await _context.Calificaciones
                .FirstOrDefaultAsync(c => c.AlumnoId == AlumnoId && c.MateriaId == MateriaId);

            if (cal == null)
            {
                cal = new Calificacion
                {
                    AlumnoId = AlumnoId,
                    MateriaId = MateriaId,
                    DocenteId = DocenteId,
                    Parcial1 = Parcial1,
                    Parcial2 = Parcial2,
                    Parcial3 = Parcial3,
                    Final = Final
                };
                _context.Calificaciones.Add(cal);
            }
            else
            {
                cal.Parcial1 = Parcial1;
                cal.Parcial2 = Parcial2;
                cal.Parcial3 = Parcial3;
                cal.Final = Final;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { id = DocenteId });
        }
    }
}