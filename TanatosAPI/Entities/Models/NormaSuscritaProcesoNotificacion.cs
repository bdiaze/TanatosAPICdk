using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[ExcludeFromCodeCoverage]
	[Table("norma_suscrita_proceso_notificacion", Schema = "tanatos")]
	public class NormaSuscritaProcesoNotificacion {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public required long Id { get; set; }

		[Required]
		[Column("id_norma_suscrita")]
		public required long IdNormaSuscrita { get; set; }

		[Required]
		[Column("id_proceso_automatico")]
		public required long IdProcesoAutomatico { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdNormaSuscrita))]
		public NormaSuscrita? NormaSuscrita { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdProcesoAutomatico))]
		public ProcesoAutomatico? ProcesoAutomatico { get; set; }
	}
}
