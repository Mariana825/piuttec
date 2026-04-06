using System.ComponentModel.DataAnnotations.Schema;

namespace piuttec.Models
{
    [Table("AlumnoGrupo")]
    public class AlumnoGrupo
    {
        public int AlumnoId { get; set; }
        public Usuario Alumno { get; set; }
        public int GrupoId { get; set; }
        public Grupo Grupo { get; set; }
    }
}
