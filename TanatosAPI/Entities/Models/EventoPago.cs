using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("evento_pago", Schema = "tanatos")]
	public class EventoPago {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("proveedor")]
		public required string Proveedor { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("evento")]
		public required string Evento { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("payload", TypeName = "jsonb")]
		public required string Payload { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("procesado")]
		public required bool Procesado { get; set; }

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
	}
}
