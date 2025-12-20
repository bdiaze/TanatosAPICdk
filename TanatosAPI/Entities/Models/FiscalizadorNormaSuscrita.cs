using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("fiscalizador_norma_suscrita", Schema = "tanatos")]
	[Comment("Tabla que contiene los fiscalizadores asociados a una norma suscrita.")]
	[Index(nameof(IdNormaSuscrita), nameof(Vigencia))]
	public class FiscalizadorNormaSuscrita {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Comment("Identificador del fiscalizador asociado a una norma suscrita.")]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_norma_suscrita")]
		[Comment("Identificador de la norma suscrita.")]
		public required long IdNormaSuscrita { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_tipo_fiscalizador")]
		[Comment("Identificador del fiscalizador asociado.")]
		public required long IdTipoFiscalizador { get; set; }

		[UseColumnAttribute]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se creó al fiscalizador asociado.")]
		public DateTime? FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se eliminó al fiscalizador asociado.")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del fiscalizador asociado.")]
		public required bool Vigencia { get; set; }

		[ForeignKey(nameof(IdNormaSuscrita))]
		public NormaSuscrita? NormaSuscrita { get; set; }

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
