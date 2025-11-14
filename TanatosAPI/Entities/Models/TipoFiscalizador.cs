using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("tipo_fiscalizador", Schema = "tanatos")]
	[Comment("Tabla que contiene los tipos de fiscalizadores de las normas.")]
	public class TipoFiscalizador {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Comment("Identificador del tipo de fiscalizador.")]
		public required long Id { get; set; }

		[Required]
		[Column("nombre")]
		[Comment("Nombre del tipo de fiscalizador.")]
		public required string Nombre { get; set; }

		[Column("nombre_corto")]
		[Comment("Nombre corto del tipo de fiscalizador.")]
		public string? NombreCorto { get; set; }

		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del tipo de fiscalizador.")]
		public required bool Vigencia { get; set; }
	}
}
