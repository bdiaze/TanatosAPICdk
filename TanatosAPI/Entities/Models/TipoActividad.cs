using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("tipo_actividad", Schema = "tanatos")]
	[Comment("Tabla que contiene las actividades que puede hacer un negocio.")]
	public class TipoActividad {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Comment("Identificador de la actividad.")]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_tipo_rubro")]
		[Comment("Identificador del rubro al que pertenece la actividad.")]
		public required long IdTipoRubro { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		[Comment("Nombre de la actividad.")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Column("descripcion")]
		[Comment("Descripción de la actividad.")]
		public string? Descripcion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia de la actividad.")]
		public required bool Vigencia { get; set; }

		[ForeignKey(nameof(IdTipoRubro))]
		public TipoRubro? TipoRubro { get; set; }

		public List<Negocio>? Negocios { get; set; }

		public List<TemplateActividad>? TemplatesActividad { get; set; }
	}
}
