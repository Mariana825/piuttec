using System.ComponentModel.DataAnnotations.Schema;

namespace piuttec.Models
{
    [Table("Materia")]
    public class Materia
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

}
