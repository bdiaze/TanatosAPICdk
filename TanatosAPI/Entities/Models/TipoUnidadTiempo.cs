using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("tipo_unidad_tiempo", Schema = "tanatos")]
	public class TipoUnidadTiempo {
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
        [Column("nombre_plural")]
        public string? NombrePlural { get; set; }

        [UseColumnAttribute]
		[Required]
		[Column("cant_segundos")]
		public required long CantSegundos { get; set; }

		[UseColumnAttribute]
		[Column("cant_minutos")]
		public long? CantMinutos { get; set; }

		[UseColumnAttribute]
		[Column("cant_horas")]
		public long? CantHoras { get; set; }

		[UseColumnAttribute]
		[Column("cant_dias")]
		public long? CantDias { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<TemplateNormaNotificacion>? TemplateNormasNotificacion { get; set; }

		[JsonIgnore]
		public List<NotificacionNormaSuscrita>? NotificacionesNormaSuscrita { get; set; }
	}
}
