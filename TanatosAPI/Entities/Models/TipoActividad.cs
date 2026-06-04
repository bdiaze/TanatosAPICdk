using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("tipo_actividad", Schema = "tanatos")]
	public class TipoActividad {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public required long Id { get; set; }

		[Required]
		[Column("id_tipo_rubro")]
		public required long IdTipoRubro { get; set; }

		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[Column("descripcion")]
		public string? Descripcion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTipoRubro))]
		public TipoRubro? TipoRubro { get; set; }

		[JsonIgnore]
		public List<Negocio>? Negocios { get; set; }

		[JsonIgnore]
		public List<TemplateActividad>? TemplatesActividad { get; set; }
	}
}
