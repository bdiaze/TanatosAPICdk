using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("template_norma_fiscalizador", Schema = "tanatos")]
	[Comment("Tabla que contiene la relación entre un template norma y un fiscalizador.")]
	[PrimaryKey(nameof(IdTemplate), nameof(IdNorma), nameof(IdTipoFiscalizador))]
	[Index(nameof(IdTipoFiscalizador))]
	public class TemplateNormaFiscalizador {
		[Required]
		[Column("id_template")]
		[Comment("Identificador del template al que pertenece la norma.")]
		public required long IdTemplate { get; set; }

		[Required]
		[Column("id_norma")]
		[Comment("Identificador de la norma asociada al template.")]
		public required long IdNorma { get; set; }

		[Required]
		[Column("id_tipo_fiscalizador")]
		[Comment("Identificador del tipo de fiscalizador.")]
		public required long IdTipoFiscalizador { get; set; }

		public TemplateNorma? TemplateNorma { get; set; }

		[ForeignKey(nameof(IdTipoFiscalizador))]
		public TipoFiscalizador? TipoFiscalizador { get; set; }

		public override int GetHashCode() {
			return HashCode.Combine(IdTemplate, IdNorma, IdTipoFiscalizador);
		}

		public override bool Equals(object? obj) {
			if (obj is not TemplateNormaFiscalizador other) {
				return false;
			}
			return IdTemplate == other.IdTemplate &&
					IdNorma == other.IdNorma &&
					IdTipoFiscalizador == other.IdTipoFiscalizador;
		}
	}
}
