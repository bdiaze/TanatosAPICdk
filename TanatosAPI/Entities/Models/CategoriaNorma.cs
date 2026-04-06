using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("categoria_norma", Schema = "tanatos")]
	[Comment("Tabla que contiene las categorías de las normas")]
	public class CategoriaNorma {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Comment("Identificador de la categoría.")]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		[Comment("Nombre de la categoría.")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Column("nombre_corto")]
		[Comment("Nombre corto de la categoría.")]
		public string? NombreCorto { get; set; }

		[UseColumnAttribute]
		[Column("descripcion")]
		[Comment("Descripción de la categoría.")]
		public string? Descripcion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia de la categoría.")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<TemplateNorma>? TemplateNormas { get; set; }

		[JsonIgnore]
		public List<NormaSuscrita>? NormasSuscritas { get; set; }
	}
}
