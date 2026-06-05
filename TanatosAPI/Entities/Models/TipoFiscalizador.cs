using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
    [Table("tipo_fiscalizador", Schema = "tanatos")]
	public class TipoFiscalizador {
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

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<TemplateNormaFiscalizador>? TemplateNormasFiscalizador { get; set; }

		[JsonIgnore]
		public List<FiscalizadorNormaSuscrita>? FiscalizadoresNormaSuscrita { get; set; }
	}
}
