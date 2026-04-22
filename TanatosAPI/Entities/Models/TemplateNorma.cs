using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("template_norma", Schema = "tanatos")]
	[Comment("Tabla que contiene las normas asociadas a un template.")]
	[PrimaryKey(nameof(IdTemplate), nameof(IdNorma))]
	public class TemplateNorma {
		[UseColumnAttribute]
		[Required]
		[Column("id_template")]
		[Comment("Identificador del template al que pertenece la norma.")]
		public required long IdTemplate { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_norma")]
		[Comment("Identificador de la norma asociada al template.")]
		public required long IdNorma { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		[Comment("Nombre de la norma.")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Column("descripcion")]
		[Comment("Descripcion de la norma.")]
		public string? Descripcion { get; set; }

		[UseColumnAttribute]
		[Column("id_tipo_periodicidad")]
		[Comment("Identificador del tipo de periodicidad asociado a la norma.")]
		public long? IdTipoPeriodicidad { get; set; }

		[UseColumnAttribute]
		[Column("multa")]
		[Comment("Multa de no cumplir con la norma")]
		public string? Multa { get; set; }

		[UseColumnAttribute]
		[Column("id_categoria_norma")]
		[Comment("Identificador de la categoría a la que pertenece la norma.")]
		public required long IdCategoriaNorma { get; set; }

		[UseColumnAttribute]
		[Column("cron_activacion_automatica")]
		[Comment("Cron que define el próximo vencimiento de la obligación al momento de la inscripción.")]
		public string? CronActivacionAutomatica { get; set; }

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
			return HashCode.Combine(IdTemplate, IdNorma, Nombre, Descripcion, IdTipoPeriodicidad, Multa, IdCategoriaNorma, CronActivacionAutomatica);
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
					CronActivacionAutomatica == other.CronActivacionAutomatica;
		}
	}
}
