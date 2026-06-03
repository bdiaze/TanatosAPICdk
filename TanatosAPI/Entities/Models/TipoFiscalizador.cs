using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("tipo_fiscalizador", Schema = "tanatos")]
	public class TipoFiscalizador {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Column("nombre_corto")]
		public string? NombreCorto { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<TemplateNormaFiscalizador>? TemplateNormasFiscalizador { get; set; }

		[JsonIgnore]
		public List<FiscalizadorNormaSuscrita>? FiscalizadoresNormaSuscrita { get; set; }
	}
}
