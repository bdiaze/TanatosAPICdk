using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
    [Table("tipo_receptor_notificacion", Schema = "tanatos")]
    public class TipoReceptorNotificacion {
        [Required]
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required long Id { get; set; }

        [Required]
        [Column("nombre")]
        public required string Nombre { get; set; }

        [Required]
        [Column("vigente")]
        public required bool Vigente { get; set; }
    }
}
