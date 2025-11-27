using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("tipo_unidad_tiempo", Schema = "tanatos")]
	[Comment("Tabla que contiene los tipos de unidades de tiempo.")]
	public class TipoUnidadTiempo {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Comment("Identificador del tipo de unidad de tiempo.")]
		public required long Id { get; set; }

		[Required]
		[Column("nombre")]
		[Comment("Nombre del tipo de unidad de tiempo.")]
		public required string Nombre { get; set; }

		[Required]
		[Column("cant_segundos")]
		[Comment("Cantidad de segundos que representan a la unidad de tiempo.")]
		public required long CantSegundos { get; set; }

		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del tipo de unidad de tiempo.")]
		public required bool Vigencia { get; set; }

		public List<TemplateNormaNotificacion>? TemplateNormasNotificacion { get; set; }
	}
}
