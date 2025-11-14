using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
    [Table("tipo_receptor_notificacion", Schema = "tanatos")]
    [Comment("Tabla que contiene los tipos de receptores de notificación.")]
    public class TipoReceptorNotificacion {
        [Required]
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Comment("Identificador del tipo de receptor de notificación.")]
        public required long Id { get; set; }

        [Required]
        [Column("nombre")]
        [Comment("Nombre del tipo de receptor de notificación.")]
        public required string Nombre { get; set; }

        [Required]
        [Column("vigencia")]
        [Comment("Vigencia del tipo de receptor de notificación.")]
        public required bool Vigencia { get; set; }

        public List<DestinatarioNotificacion>? DestinatariosNotificaciones { get; set; }
    }
}
