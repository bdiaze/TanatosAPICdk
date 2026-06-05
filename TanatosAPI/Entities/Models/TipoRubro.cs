using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
    [Table("tipo_rubro", Schema = "tanatos")]
	public class TipoRubro {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public required long Id { get; set; }

		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[Column("descripcion")]
		public string? Descripcion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<TipoActividad>? TiposActividades { get; set; }
	}
}
