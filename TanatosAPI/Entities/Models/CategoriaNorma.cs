using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
    [Table("categoria_norma", Schema = "tanatos")]
	public class CategoriaNorma {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public required long Id { get; set; }

		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[Column("nombre_corto")]
		public string? NombreCorto { get; set; }

		[Column("descripcion")]
		public string? Descripcion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<TemplateNorma>? TemplateNormas { get; set; }

		[JsonIgnore]
		public List<NormaSuscrita>? NormasSuscritas { get; set; }
	}
}
