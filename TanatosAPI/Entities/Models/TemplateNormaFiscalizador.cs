using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("template_norma_fiscalizador", Schema = "tanatos")]
	[Comment("Tabla que contiene la relación entre un template norma y un fiscalizador.")]
	[PrimaryKey(nameof(IdTemplateNorma), nameof(IdTipoFiscalizador))]
	[Index(nameof(IdTipoFiscalizador))]
	public class TemplateNormaFiscalizador {
		[Required]
		[Column("id_template_norma")]
		[Comment("Identificador de la norma perteneciente a un template.")]
		public required long IdTemplateNorma { get; set; }

		[Required]
		[Column("id_tipo_fiscalizador")]
		[Comment("Identificador del tipo de fiscalizador.")]
		public required long IdTipoFiscalizador { get; set; }

		[ForeignKey(nameof(IdTemplateNorma))]
		public TemplateNorma? TemplateNorma { get; set; }

		[ForeignKey(nameof(IdTipoFiscalizador))]
		public TipoFiscalizador? TipoFiscalizador { get; set; }
	}
}
