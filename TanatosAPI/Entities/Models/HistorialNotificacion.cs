using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("historial_notificacion", Schema = "tanatos")]
	public class HistorialNotificacion {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_historial_norma_suscrita")]
		public required long IdHistorialNormaSuscrita { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_destinatario_notificacion")]
		public required long IdDestinatarioNotificacion { get; set; }

		[UseColumnAttribute]
		[Column("id_tipo_unidad_tiempo_antelacion")]
		public long? IdTipoUnidadTiempoAntelacion { get; set; }

		[UseColumnAttribute]
		[Column("cant_antelacion")]
		public int? CantAntelacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_programacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaProgramacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_ejecucion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEjecucion { get; set; }

		[UseColumnAttribute]
		[Column("estado")]
		public short? Estado { get; set; }

		[UseColumnAttribute]
		[Column("observacion")]
		public string? Observacion { get; set; }

        [UseColumnAttribute]
        [Column("codigo_acceso")]
        public string? CodigoAcceso { get; set; }

        [UseColumnAttribute]
        [Column("fecha_caducidad_codigo_acceso", TypeName = "timestamp with time zone")]
        public DateTime? FechaCaducidadCodigoAcceso { get; set; }

        [UseColumnAttribute]
		[Column("hermes_id_mensaje")]
		public string? HermesIdMensaje { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
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
