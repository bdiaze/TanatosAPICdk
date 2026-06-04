using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("template_norma_fiscalizador", Schema = "tanatos")]
	public class TemplateNormaFiscalizador {
		[Required]
		[Column("id_template")]
		public required long IdTemplate { get; set; }

		[Required]
		[Column("id_norma")]
		public required long IdNorma { get; set; }

		[Required]
		[Column("id_tipo_fiscalizador")]
		public required long IdTipoFiscalizador { get; set; }

		[JsonIgnore]
		public TemplateNorma? TemplateNorma { get; set; }

		[JsonIgnore]
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
