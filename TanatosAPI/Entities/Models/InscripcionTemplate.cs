using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("inscripcion_template", Schema = "tanatos")]
	public class InscripcionTemplate {
		[Required]
		[Column("sub")]
		public required string Sub { get; set; }

		[Required]
		[Column("id_negocio")]
		public required long IdNegocio { get; set; }

		[Required]
		[Column("id_template")]
		public required long IdTemplate { get; set; }

		[Required]
		[Column("fecha_activacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaActivacion { get; set; }

		[Column("fecha_desactivacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaDesactivacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTemplate))]
		public Template? Template { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdNegocio))]
		public Negocio? Negocio { get; set; }
	}
}
