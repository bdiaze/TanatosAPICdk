using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("inscripcion_template", Schema = "tanatos")]
	[Comment("Tabla que contiene los templates a los que un usuario está inscrito.")]
	[PrimaryKey(nameof(Sub), nameof(IdTemplate))]
	[Index(nameof(IdTemplate))]
	public class InscripcionTemplate {
		[Required]
		[Column("sub")]
		[Comment("Usuario al que está asociada la inscripción.")]
		public required string Sub { get; set; }

		[Required]
		[Column("id_template")]
		[Comment("Identificador del template al que está inscrito el usuario.")]
		public required long IdTemplate { get; set; }

		[Required]
		[Column("fecha_activacion")]
		[Comment("Fecha en que se activa la inscripción.")]
		public required DateTimeOffset FechaActivacion { get; set; }

		[Column("fecha_desactivacion")]
		[Comment("Fecha en que se desactiva la inscripción.")]
		public DateTimeOffset? FechaDesactivacion { get; set; }

		[Required]
		[Column("vigencia")]
		[Comment("Vigencia de la inscripción.")]
		public required bool Vigencia { get; set; }

		[ForeignKey(nameof(IdTemplate))]
		public Template? Template { get; set; }
	}
}
