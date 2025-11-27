using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("template_norma", Schema = "tanatos")]
	[Comment("Tabla que contiene las normas asociadas a un template.")]
	public class TemplateNorma {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Comment("Identificador de la norma asociada al template.")]
		public required long Id { get; set; }

		[Required]
		[Column("id_template")]
		[Comment("Identificador del template al que pertenece la norma.")]
		public required long IdTemplate { get; set; }

		[Required]
		[Column("nombre")]
		[Comment("Nombre de la norma.")]
		public required string Nombre { get; set; }

		[Column("descripcion")]
		[Comment("Descripcion de la norma.")]
		public string? Descripcion { get; set; }

		[Column("id_tipo_periodicidad")]
		[Comment("Identificador del tipo de periodicidad asociado a la norma.")]
		public long? IdTipoPeriodicidad { get; set; }

		[Column("multa")]
		[Comment("Multa de no cumplir con la norma")]
		public string? Multa { get; set; }

		[Column("id_categoria_norma")]
		[Comment("Identificador de la categoría a la que pertenece la norma.")]
		public required long IdCategoriaNorma { get; set; }

		[ForeignKey(nameof(IdTemplate))]
		public Template? Template { get; set; }

		[ForeignKey(nameof(IdTipoPeriodicidad))]
		public TipoPeriodicidad? TipoPeriodicidad { get; set; }

		[ForeignKey(nameof(IdCategoriaNorma))]
		public CategoriaNorma? CategoriaNorma { get; set; }

		public List<TemplateNormaFiscalizador>? TemplateNormaFiscalizadores { get; set; }

		public List<TemplateNormaNotificacion>? TemplateNormaNotificaciones { get; set; }
	}
}
