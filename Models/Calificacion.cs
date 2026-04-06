using System.ComponentModel.DataAnnotations.Schema;

namespace piuttec.Models
{
    [Table("Calificacion")]
    public class Calificacion
    {
        public int Id { get; set; }
        public int AlumnoId { get; set; }
        public Usuario Alumno { get; set; }
        public int MateriaId { get; set; }
        public Materia Materia { get; set; }
        public int DocenteId { get; set; }
        public Usuario Docente { get; set; }

        public double Parcial1 { get; set; }
        public double Parcial2 { get; set; }
        public double Parcial3 { get; set; }
        public double Final { get; set; }

        // Calcula automáticamente el promedio
        public double Promedio => (Parcial1 + Parcial2 + Parcial3 + Final) / 4;
    }
}
