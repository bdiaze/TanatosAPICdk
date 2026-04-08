using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("historial_notificacion", Schema = "tanatos")]
	[Comment("Tabla que contiene el historial de notificaciones de una norma suscrita.")]
	public class HistorialNotificacion {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Comment("Identificador del historial de notificación de una norma suscrita.")]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_historial_norma_suscrita")]
		[Comment("Identificador del historial de ejecución de una norma suscrita.")]
		public required long IdHistorialNormaSuscrita { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_destinatario_notificacion")]
		[Comment("Identificador del destinatario de la notificación.")]
		public required long IdDestinatarioNotificacion { get; set; }

		[UseColumnAttribute]
		[Column("id_tipo_unidad_tiempo_antelacion")]
		[Comment("Identificador del tipo de unidad de tiempo correspondiente a la notificación.")]
		public long? IdTipoUnidadTiempoAntelacion { get; set; }

		[UseColumnAttribute]
		[Column("cant_antelacion")]
		[Comment("Cantidad de unidades de tiempo correspondientes a la notificación.")]
		public int? CantAntelacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_programacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se programó el envío de la notificación.")]
		public required DateTime FechaProgramacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_ejecucion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se ejecutó el envío de la notificación.")]
		public DateTime? FechaEjecucion { get; set; }

		[UseColumnAttribute]
		[Column("estado")]
		[Comment("Estado de la notificación - 0: Pendiente - 1: Enviado - 2: Omitido.")]
		public short? Estado { get; set; }

		[UseColumnAttribute]
		[Column("observacion")]
		[Comment("Observación relacionada a la notificación.")]
		public string? Observacion { get; set; }

		[UseColumnAttribute]
		[Column("hermes_id_mensaje")]
		[Comment("ID del mensaje en Hermes.")]
		public string? HermesIdMensaje { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se creó el registro.")]
		public required DateTime FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se eliminó el registro.")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del registro.")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdHistorialNormaSuscrita))]
		public HistorialNormaSuscrita? HistorialNormaSuscrita { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdDestinatarioNotificacion))]
		public DestinatarioNotificacion? DestinatarioNotificacion { get; set; }
	}
}
