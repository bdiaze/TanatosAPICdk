using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("tipo_actividad", Schema = "tanatos")]
	public class TipoActividad {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_tipo_rubro")]
		public required long IdTipoRubro { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Column("descripcion")]
		public string? Descripcion { get; set; }

		[UseColumnAttribute]
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
