using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("fiscalizador_norma_suscrita", Schema = "tanatos")]
	public class FiscalizadorNormaSuscrita {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_norma_suscrita")]
		public required long IdNormaSuscrita { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_tipo_fiscalizador")]
		public required long IdTipoFiscalizador { get; set; }

		[UseColumnAttribute]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdNormaSuscrita))]
		public NormaSuscrita? NormaSuscrita { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTipoFiscalizador))]
		public TipoFiscalizador? TipoFiscalizador { get; set; }

		public override int GetHashCode() {
			return HashCode.Combine(Id, IdNormaSuscrita, IdTipoFiscalizador);
		}

		public override bool Equals(object? obj) {
			if (obj is not FiscalizadorNormaSuscrita other) {
				return false;
			}
			return Id == other.Id &&
				   IdNormaSuscrita == other.IdNormaSuscrita &&
				   IdTipoFiscalizador == other.IdTipoFiscalizador;
		}
	}
}
