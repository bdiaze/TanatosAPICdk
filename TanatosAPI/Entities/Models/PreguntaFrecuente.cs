using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Models {
	[ExcludeFromCodeCoverage]
	[Table("pregunta_frecuente", Schema = "tanatos")]
	public class PreguntaFrecuente {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public required long Id { get; set; }

		[Required]
		[Column("pregunta")]
		public required string Pregunta { get; set; }

		[Required]
		[Column("respuesta")]
		public required string Respuesta { get; set; }

		[Required]
		[Column("habilitado")]
		public required bool Habilitado { get; set; }

		[Required]
		[Column("orden")]
		public required int Orden { get; set; }

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
