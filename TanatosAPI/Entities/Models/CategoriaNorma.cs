using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("categoria_norma", Schema = "tanatos")]
	[Comment("Tabla que contiene las categorías de las normas")]
	public class CategoriaNorma {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Comment("Identificador de la categoría.")]
		public required long Id { get; set; }

		[Required]
		[Column("nombre")]
		[Comment("Nombre de la categoría.")]
		public required string Nombre { get; set; }

		[Column("nombre_corto")]
		[Comment("Nombre corto de la categoría.")]
		public string? NombreCorto { get; set; }

		[Column("descripcion")]
		[Comment("Descripción de la categoría.")]
		public string? Descripcion { get; set; }

		[Required]
		[Column("vigencia")]
		[Comment("Vigencia de la categoría.")]
		public required bool Vigencia { get; set; }

		public List<TemplateNorma>? TemplateNormas { get; set; } 
	}
}
