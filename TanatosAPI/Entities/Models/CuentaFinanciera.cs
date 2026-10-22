using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[ExcludeFromCodeCoverage]
	[Table("cuenta_financiera", Schema = "tanatos")]
	public class CuentaFinanciera {
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

		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[Required]
		[Column("id_tipo_cuenta_financiera")]
		public required long IdTipoCuentaFinanciera { get; set; }

		[Required]
		[Column("saldo_inicial")]
		public required decimal SaldoInicial { get; set; }

		[Required]
		[Column("fecha_saldo_inicial", TypeName = "timestamp with time zone")]
		public required DateTime FechaSaldoInicial { get; set; }

		[Required]
		[Column("saldo_actual")]
		public required decimal SaldoActual { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTipoCuentaFinanciera))]
		public TipoCuentaFinanciera? TipoCuentaFinanciera { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdNegocio))]
		public Negocio? Negocio { get; set; }

		[JsonIgnore]
		public List<MovimientoFinanciero>? MovimientosFinancieros { get; set; }
	}
}
