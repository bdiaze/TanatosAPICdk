using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[ExcludeFromCodeCoverage]
	[Table("tarea", Schema = "tanatos")]
	public class Tarea {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[Required]
		[Column("id_cliente")]
		public required long IdCliente { get; set; }

		[Required]
		[Column("descripcion")]
		public required string Descripcion { get; set; }

		[Required]
		[Column("fecha_vencimiento", TypeName = "timestamp with time zone")]
		public required DateTime FechaVencimiento { get; set; }

		[Required]
		[Column("estado")]
		// 0: Pendiente - 1: Completada - 2: Cancelada
		public required short Estado { get; set; }

		[Column("fecha_completitud", TypeName = "timestamp with time zone")]
		public DateTime? FechaCompletitud { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdCliente))]
		public Cliente? Cliente { get; set; }
	}
}
