using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
    [Table("evento_pago", Schema = "tanatos")]
	public class EventoPago {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[Required]
		[Column("proveedor")]
		public required string Proveedor { get; set; }

		[Required]
		[Column("evento")]
		public required string Evento { get; set; }

		[Required]
		[Column("payload", TypeName = "jsonb")]
		public required string Payload { get; set; }

		[Required]
		[Column("procesado")]
		public required bool Procesado { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }
	}
}
