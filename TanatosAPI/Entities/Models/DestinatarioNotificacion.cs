using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
    [Table("destinatario_notificacion", Schema = "tanatos")]
    [Comment("Tabla que contiene los destinatarios de las notificaciones de un usuario.")]
    [Index(nameof(Sub), nameof(IdNegocio), nameof(IdTipoReceptor))]
    [Index(nameof(IdTipoReceptor))]
    [Index(nameof(CodigoValidacion), IsUnique = true)]
    public class DestinatarioNotificacion {
		[UseColumnAttribute]
		[Required]
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Comment("Identificador del destinatario de notificación.")]
        public long Id { get; set; }

		[UseColumnAttribute]
		[Required]
        [Column("sub")]
        [Comment("Usuario al que pertenece el destinatario de notificación.")]
        public required string Sub { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_negocio")]
		[Comment("Identificador del negocio del usuario.")]
		public required long IdNegocio { get; set; }

		[UseColumnAttribute]
		[Required]
        [Column("id_tipo_receptor")]
        [Comment("Identificador del tipo de receptor asociado al destinatario.")]
        public required long IdTipoReceptor { get; set; }

		[UseColumnAttribute]
		[Required]
        [Column("destino")]
        [Comment("Destino de la notificación. Puede ser un correo o un número de Whatsapp.")]
        public required string Destino { get; set; }

		[UseColumnAttribute]
		[Required]
        [Column("codigo_validacion")]
        [Comment("Código generado para validar que el destinatario se suscribe a la notificación.")]
        public required string CodigoValidacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_caducidad_codigo_validacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que caduca el código de validación.")]
		public required DateTime FechaCaducidadCodigoValidacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_validacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se validó el destinatario.")]
		public DateTime? FechaValidacion { get; set; }

		[UseColumnAttribute]
		[Required]
        [Column("validado")]
        [Comment("Identifica si el destinatario ya fue validado.")]
        public required bool Validado { get; set; }

		[UseColumnAttribute]
		[Required]
        [Column("fecha_creacion", TypeName = "timestamp with time zone")]
        [Comment("Fecha en que se creó el destinatario.")]
        public required DateTime FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
        [Comment("Fecha en que se eliminó el destinatario.")]
        public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
		[Required]
        [Column("vigencia")]
        [Comment("Vigencia del destinatario.")]
        public required bool Vigencia { get; set; }

        [ForeignKey(nameof(IdTipoReceptor))]
        public TipoReceptorNotificacion? TipoReceptorNotificacion { get; set; }

        [ForeignKey(nameof(IdNegocio))]
		public Negocio? Negocio { get; set; }

        public List<HistorialNotificacion>? HistorialNotificaciones { get; set; }
    }
}
