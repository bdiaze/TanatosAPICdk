using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Models {
	[ExcludeFromCodeCoverage]
	[Table("video_tutorial", Schema = "tanatos")]
	public class VideoTutorial {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public required long Id { get; set; }

		[Required]
		[Column("titulo")]
		public required string Titulo { get; set; }

		[Required]
		[Column("descripcion")]
		public string? Descripcion { get; set; }

		[Required]
		[Column("url")]
		public required string Url { get; set; }

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
