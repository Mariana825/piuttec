namespace piuttec.Models
{

    public class DocenteMateria
    {
        public int DocenteId { get; set; }
        public Usuario Docente { get; set; }
        public int MateriaId { get; set; }
        public Materia Materia { get; set; }
    }
}
