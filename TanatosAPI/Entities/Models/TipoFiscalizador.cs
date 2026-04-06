using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("tipo_fiscalizador", Schema = "tanatos")]
	[Comment("Tabla que contiene los tipos de fiscalizadores de las normas.")]
	public class TipoFiscalizador {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Comment("Identificador del tipo de fiscalizador.")]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		[Comment("Nombre del tipo de fiscalizador.")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Column("nombre_corto")]
		[Comment("Nombre corto del tipo de fiscalizador.")]
		public string? NombreCorto { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del tipo de fiscalizador.")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<TemplateNormaFiscalizador>? TemplateNormasFiscalizador { get; set; }

		[JsonIgnore]
		public List<FiscalizadorNormaSuscrita>? FiscalizadoresNormaSuscrita { get; set; }
	}
}
