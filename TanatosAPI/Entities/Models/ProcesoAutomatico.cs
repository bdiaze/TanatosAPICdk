using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[ExcludeFromCodeCoverage]
	[Table("proceso_automatico", Schema = "tanatos")]
	public class ProcesoAutomatico {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public required long Id { get; set; }

		[Required]
		[Column("id_tipo_proceso_automatico")]
		public required long IdTipoProcesoAutomatico { get; set; }

		[Required]
		[Column("id_proceso_kairos")]
		public required string IdProcesoKairos { get; set; }

		[Required]
		[Column("id_calendarizacion_kairos")]
		public required string IdCalendarizacionKairos { get; set; }

		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[Required]
		[Column("arn_rol")]
		public required string ArnRol { get; set; }

		[Required]
		[Column("arn_proceso")]
		public required string ArnProceso { get; set; }

		[Required]
		[Column("parametros")]
		public required string Parametros { get; set; }

		[Column("cron")]
		public string? Cron { get; set; }

		[Column("frecuencia_dias")]
		public int? FrecuenciaDias { get; set; }

		[Column("inicio_ejecucion_utc", TypeName = "timestamp with time zone")]
		public DateTime? InicioEjecucionUtc { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTipoProcesoAutomatico))]
		public TipoProcesoAutomatico? TipoProcesoAutomatico { get; set; }

		[JsonIgnore]
		public List<NormaSuscritaProcesoNotificacion>? NormaSuscritaProcesoNotificacion { get; set; }
	}
}
