using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("tipo_periodicidad", Schema = "tanatos")]
	[Comment("Tabla que contiene los tipos de periodicidad.")]
	public class TipoPeriodicidad {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Comment("Identificador del tipo de periodicidad.")]
		public required long Id { get; set; }

		[Required]
		[Column("nombre")]
		[Comment("Nombre del tipo de periodicidad.")]
		public required string Nombre { get; set; }

		[Column("descripcion")]
		[Comment("Descripción del tipo de periodicidad.")]
		public string? Descripcion { get; set; }

		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del tipo de periodicidad.")]
		public required bool Vigencia { get; set; }

		public List<TemplateNorma>? TemplateNormas { get; set; }

	}
}
