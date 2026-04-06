using System.ComponentModel.DataAnnotations.Schema;

namespace piuttec.Models
{
    [Table("Grupo")]
    public class Grupo
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}
