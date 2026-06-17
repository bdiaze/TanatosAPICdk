using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
    [Table("template_norma", Schema = "tanatos")]
	public class TemplateNorma {
		[Required]
		[Column("id_template")]
		public required long IdTemplate { get; set; }

		[Required]
		[Column("id_norma")]
		public required long IdNorma { get; set; }

		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[Column("descripcion")]
		public string? Descripcion { get; set; }

		[Column("id_tipo_periodicidad")]
		public long? IdTipoPeriodicidad { get; set; }

		[Column("multa")]
		public string? Multa { get; set; }

		[Column("id_categoria_norma")]
		public required long IdCategoriaNorma { get; set; }

		[Column("cron_activacion_automatica")]
		public string? CronActivacionAutomatica { get; set; }

		[Column("dias_activacion_automatica")]
		public int? DiasActivacionAutomatica { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTemplate))]
		public Template? Template { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTipoPeriodicidad))]
		public TipoPeriodicidad? TipoPeriodicidad { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdCategoriaNorma))]
		public CategoriaNorma? CategoriaNorma { get; set; }

		public List<TemplateNormaFiscalizador>? TemplateNormaFiscalizadores { get; set; }

		public List<TemplateNormaNotificacion>? TemplateNormaNotificaciones { get; set; }

		[JsonIgnore]
		public List<NormaSuscrita>? NormasSuscritas { get; set; }

		public override int GetHashCode() {
			int hash = HashCode.Combine(IdTemplate, IdNorma, Nombre, Descripcion, IdTipoPeriodicidad, Multa, IdCategoriaNorma, CronActivacionAutomatica);
			return HashCode.Combine(hash, DiasActivacionAutomatica);
		}

		public override bool Equals(object? obj) {
			if (obj is not TemplateNorma other) {
				return false;
			}
			return IdTemplate == other.IdTemplate &&
					IdNorma == other.IdNorma &&
					Nombre == other.Nombre &&
					Descripcion == other.Descripcion &&
					IdTipoPeriodicidad == other.IdTipoPeriodicidad &&
					Multa == other.Multa &&
					IdCategoriaNorma == other.IdCategoriaNorma &&
					CronActivacionAutomatica == other.CronActivacionAutomatica &&
					DiasActivacionAutomatica == other.DiasActivacionAutomatica;
		}
	}
}
