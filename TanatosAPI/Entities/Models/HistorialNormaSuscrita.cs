using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("historial_norma_suscrita", Schema = "tanatos")]
	[Comment("Tabla que contiene el historial de ejecución de una norma suscrita.")]
	[Index(nameof(IdNormaSuscrita), nameof(FechaVencimiento))]
	[Index(nameof(FechaVencimiento))]
	public class HistorialNormaSuscrita {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Comment("Identificador del historial de ejecución de una norma suscrita.")]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_norma_suscrita")]
		[Comment("Identificador de la norma suscrita.")]
		public required long IdNormaSuscrita { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_vencimiento", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que vencerá la ejecución de la norma suscrita")]
		public required DateTime FechaVencimiento { get; set; }

		[UseColumnAttribute]
		[Column("fecha_completitud", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se completó la ejecución de la norma suscrita.")]
		public DateTime? FechaCompletitud { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se creó el registro.")]
		public required DateTime FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se eliminó el registro.")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del registro.")]
		public required bool Vigencia { get; set; }

		[ForeignKey(nameof(IdNormaSuscrita))]
		public NormaSuscrita? NormaSuscrita { get; set; }

		public List<HistorialNotificacion>? HistorialNotificaciones { get; set; }
	}
}
