using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("template_actividad", Schema = "tanatos")]
	[Comment("Tabla que contiene la recomendación de templates según tipo de actividad de un negocio.")]
	[PrimaryKey(nameof(IdTemplate), nameof(IdTipoActividad))]
	public class TemplateActividad {
		[UseColumnAttribute]
		[Required]
		[Column("id_template")]
		[Comment("Identificador del template.")]
		public required long IdTemplate { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_tipo_actividad")]
		[Comment("Identificador del tipo de actividad del negocio.")]
		public required long IdTipoActividad { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTemplate))]
		public Template? Template { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTipoActividad))]
		public TipoActividad? TipoActividad { get; set; }
	}
}
