using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
    [Table("destinatario_notificacion", Schema = "tanatos")]
    [Comment("Tabla que contiene los destinatarios de las notificaciones de un usuario.")]
    [Index(nameof(Sub), nameof(IdTipoReceptor))]
    [Index(nameof(IdTipoReceptor))]
    [Index(nameof(CodigoValidacion), IsUnique = true)]
    public class DestinatarioNotificacion {
        [Required]
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Comment("Identificador del destinatario de notificación.")]
        public long Id { get; set; }

        [Required]
        [Column("sub")]
        [Comment("Usuario al que pertenece el destinatario de notificación.")]
        public required string Sub { get; set; }

        [Required]
        [Column("id_tipo_receptor")]
        [Comment("Identificador del tipo de receptor asociado al destinatario.")]
        public required long IdTipoReceptor { get; set; }

        [Required]
        [Column("destino")]
        [Comment("Destino de la notificación. Puede ser un correo o un número de Whatsapp.")]
        public required string Destino { get; set; }

        [Required]
        [Column("codigo_validacion")]
        [Comment("Código generado para validar que el destinatario se suscribe a la notificación.")]
        public required string CodigoValidacion { get; set; }

		[Required]
		[Column("fecha_caducidad_codigo_validacion")]
		[Comment("Fecha en que caduca el código de validación.")]
		public required DateTime FechaCaducidadCodigoValidacion { get; set; }


		[Column("fecha_validacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se validó el destinatario.")]
		public DateTime? FechaValidacion { get; set; }

		[Required]
        [Column("validado")]
        [Comment("Identifica si el destinatario ya fue validado.")]
        public required bool Validado { get; set; }

        [Required]
        [Column("fecha_creacion", TypeName = "timestamp with time zone")]
        [Comment("Fecha en que se creó el destinatario.")]
        public required DateTime FechaCreacion { get; set; }

        [Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
        [Comment("Fecha en que se eliminó el destinatario.")]
        public DateTime? FechaEliminacion { get; set; }

        [Required]
        [Column("vigencia")]
        [Comment("Vigencia del destinatario.")]
        public required bool Vigencia { get; set; }

        [ForeignKey(nameof(IdTipoReceptor))]
        public TipoReceptorNotificacion? TipoReceptorNotificacion { get; set; }
    }
}
