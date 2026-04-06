using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("tipo_unidad_tiempo", Schema = "tanatos")]
	[Comment("Tabla que contiene los tipos de unidades de tiempo.")]
	public class TipoUnidadTiempo {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Comment("Identificador del tipo de unidad de tiempo.")]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		[Comment("Nombre del tipo de unidad de tiempo.")]
		public required string Nombre { get; set; }

        [UseColumnAttribute]
        [Column("nombre_plural")]
        [Comment("Nombre plural del tipo de unidad de tiempo.")]
        public string? NombrePlural { get; set; }

        [UseColumnAttribute]
		[Required]
		[Column("cant_segundos")]
		[Comment("Cantidad de segundos que representan a la unidad de tiempo.")]
		public required long CantSegundos { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del tipo de unidad de tiempo.")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<TemplateNormaNotificacion>? TemplateNormasNotificacion { get; set; }

		[JsonIgnore]
		public List<NotificacionNormaSuscrita>? NotificacionesNormaSuscrita { get; set; }
	}
}
