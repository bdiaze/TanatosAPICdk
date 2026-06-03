using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("template_actividad", Schema = "tanatos")]
	public class TemplateActividad {
		[UseColumnAttribute]
		[Required]
		[Column("id_template")]
		public required long IdTemplate { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_tipo_actividad")]
		public required long IdTipoActividad { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTemplate))]
		public Template? Template { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTipoActividad))]
		public TipoActividad? TipoActividad { get; set; }
	}
}
