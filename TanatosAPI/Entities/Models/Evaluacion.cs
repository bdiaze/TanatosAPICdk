using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Models {
	[ExcludeFromCodeCoverage]
	[Table("evaluacion", Schema = "tanatos")]
	public class Evaluacion {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[Required]
		[Column("sub")]
		public required string Sub { get; set; }

		[Required]
		[Column("puntaje")]
		public required short Puntaje { get; set; }

		[Column("comentario")]
		public string? Comentario { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }
	}
}
