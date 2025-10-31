using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
    [Table("tipo_receptor_notificacion", Schema = "tanatos")]
    public class TipoReceptorNotificacion {
        [Required]
        [Column("id")]
        [Key]
        public long Id { get; set; }

        [Required]
        [Column("nombre")]
        public string Nombre { get; set; }

        [Required]
        [Column("vigente")]
        public bool Vigente { get; set; }
    }
}
