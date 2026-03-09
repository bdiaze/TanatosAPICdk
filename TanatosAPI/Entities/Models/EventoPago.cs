using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TanatosAPI.Entities.Models {
	[Table("evento_pago", Schema = "tanatos")]
	[Comment("Tabla que contiene los eventos de pagos recepcionados.")]
	public class EventoPago {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Comment("Identificador del evento de pago.")]
		public long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("proveedor")]
		[Comment("Proveedor que informa el evento de pago.")]
		public required string Proveedor { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("evento")]
		[Comment("Tipo de evento.")]
		public required string Evento { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("payload", TypeName = "jsonb")]
		[Comment("Payload del evento recepcionado desde el proveedor.")]
		public required JsonDocument Payload { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("procesado")]
		[Comment("Indicador de si evento fue procesado.")]
		public required bool Procesado { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se creó el evento de pago.")]
		public required DateTime FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se eliminó el evento de pago.")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del evento de pago.")]
		public required bool Vigencia { get; set; }
	}
}
