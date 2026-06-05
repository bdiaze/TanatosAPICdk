using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
    [Table("tipo_unidad_tiempo", Schema = "tanatos")]
	public class TipoUnidadTiempo {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public required long Id { get; set; }

		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

        [Column("nombre_plural")]
        public string? NombrePlural { get; set; }

		[Required]
		[Column("cant_segundos")]
		public required long CantSegundos { get; set; }

		[Column("cant_minutos")]
		public long? CantMinutos { get; set; }

		[Column("cant_horas")]
		public long? CantHoras { get; set; }

		[Column("cant_dias")]
		public long? CantDias { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<TemplateNormaNotificacion>? TemplateNormasNotificacion { get; set; }

		[JsonIgnore]
		public List<NotificacionNormaSuscrita>? NotificacionesNormaSuscrita { get; set; }
	}
}
