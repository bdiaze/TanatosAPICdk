using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[ExcludeFromCodeCoverage]
	[Table("modelo_canvas", Schema = "tanatos")]
	public class ModeloCanvas {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[Required]
		[Column("sub")]
		public required string Sub { get; set; }

		[Required]
		[Column("id_negocio")]
		public required long IdNegocio { get; set; }

		[Column("socios_clave")]
		public string? SociosClave { get; set; }

		[Column("actividades_clave")]
		public string? ActividadesClave { get; set; }

		[Column("recursos_clave")]
		public string? RecursosClave { get; set; }

		[Column("propuesta_valor")]
		public string? PropuestaValor { get; set; }

		[Column("relaciones_clientes")]
		public string? RelacionesClientes { get; set; }

		[Column("canales")]
		public string? Canales { get; set; }

		[Column("segmentos_clientes")]
		public string? SegmentosClientes { get; set; }

		[Column("estructura_costos")]
		public string? EstructuraCostos { get; set; }

		[Column("fuentes_ingresos")]
		public string? FuentesIngresos { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdNegocio))]
		public Negocio? Negocio { get; set; }
	}
}
