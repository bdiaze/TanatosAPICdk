using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("tipo_rubro", Schema = "tanatos")]
	[Comment("Tabla que contiene los rubros a los que puede pertenecer un negocio.")]
	public class TipoRubro {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Comment("Identificador del rubro.")]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		[Comment("Nombre del rubro.")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Column("descripcion")]
		[Comment("Descripción del rubro.")]
		public string? Descripcion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del rubro.")]
		public required bool Vigencia { get; set; }

		public List<TipoActividad>? TiposActividades { get; set; }
	}
}
