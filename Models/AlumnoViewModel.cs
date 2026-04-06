namespace piuttec.Models
{
    public class AlumnoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Grupo { get; set; }
        public List<string> Materias { get; set; }
        public List<Calificacion> Calificaciones { get; set; } // Parcial1, Parcial2, Parcial3, Final, Promedio
    }
}
