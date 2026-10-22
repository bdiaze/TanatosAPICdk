using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[ExcludeFromCodeCoverage]
	[Table("movimiento_financiero", Schema = "tanatos")]
	public class MovimientoFinanciero {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[Required]
		[Column("id_cuenta_financiera")]
		public required long IdCuentaFinanciera { get; set; }

		[Column("id_categoria_financiera")]
		public long? IdCategoriaFinanciera { get; set; }

		[Required]
		[Column("tipo_movimiento")]
		public required short TipoMovimiento { get; set; }

		[Required]
		[Column("monto")]
		public required decimal Monto { get; set; }

		[Required]
		[Column("saldo")]
		public required decimal Saldo { get; set; }

		[Required]
		[Column("fecha_movimiento", TypeName = "timestamp with time zone")]
		public required DateTime FechaMovimiento { get; set; }

		[Column("descripcion")]
		public string? Descripcion { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdCuentaFinanciera))]
		public CuentaFinanciera? CuentaFinanciera { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdCategoriaFinanciera))]
		public CategoriaFinanciera? CategoriaFinanciera { get; set; }
	}
}
