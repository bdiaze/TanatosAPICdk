using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
    [Table("historial_notificacion", Schema = "tanatos")]
	public class HistorialNotificacion {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public required long Id { get; set; }

		[Required]
		[Column("id_historial_norma_suscrita")]
		public required long IdHistorialNormaSuscrita { get; set; }

		[Required]
		[Column("id_destinatario_notificacion")]
		public required long IdDestinatarioNotificacion { get; set; }

		[Column("id_tipo_unidad_tiempo_antelacion")]
		public long? IdTipoUnidadTiempoAntelacion { get; set; }

		[Column("cant_antelacion")]
		public int? CantAntelacion { get; set; }

		[Required]
		[Column("fecha_programacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaProgramacion { get; set; }

		[Column("fecha_ejecucion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEjecucion { get; set; }

		[Column("estado")]
		public short? Estado { get; set; }

		[Column("observacion")]
		public string? Observacion { get; set; }

        [Column("codigo_acceso")]
        public string? CodigoAcceso { get; set; }

        [Column("fecha_caducidad_codigo_acceso", TypeName = "timestamp with time zone")]
        public DateTime? FechaCaducidadCodigoAcceso { get; set; }

		[Column("hermes_id_mensaje")]
		public string? HermesIdMensaje { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdHistorialNormaSuscrita))]
		public HistorialNormaSuscrita? HistorialNormaSuscrita { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdDestinatarioNotificacion))]
		public DestinatarioNotificacion? DestinatarioNotificacion { get; set; }
	}
}
