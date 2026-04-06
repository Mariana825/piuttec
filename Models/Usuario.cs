using System.ComponentModel.DataAnnotations.Schema;

namespace piuttec.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; } // Admin, Alumno, Docente
        public string Contrasena { get; set; } // Para login
    }
}
