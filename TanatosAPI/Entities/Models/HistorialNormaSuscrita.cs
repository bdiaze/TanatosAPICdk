using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
    [Table("historial_norma_suscrita", Schema = "tanatos")]
	public class HistorialNormaSuscrita {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public required long Id { get; set; }

		[Required]
		[Column("id_norma_suscrita")]
		public required long IdNormaSuscrita { get; set; }

		[Required]
		[Column("fecha_vencimiento", TypeName = "timestamp with time zone")]
		public required DateTime FechaVencimiento { get; set; }

		[Column("fecha_completitud", TypeName = "timestamp with time zone")]
		public DateTime? FechaCompletitud { get; set; }

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
		public List<HistorialNotificacion>? HistorialNotificaciones { get; set; }

		[JsonIgnore]
		public List<DocumentoAdjunto>? DocumentosAdjuntos { get; set; }
	}
}
