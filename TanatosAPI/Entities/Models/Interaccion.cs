using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[ExcludeFromCodeCoverage]
	[Table("interaccion", Schema = "tanatos")]
	public class Interaccion {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[Required]
		[Column("id_cliente")]
		public required long IdCliente { get; set; }

		[Column("id_cliente_relacionado")]
		public long? IdClienteRelacionado { get; set; }

		[Column("id_contacto_relacionado")]
		public long? IdContactoRelacionado { get; set; }

		[Required]
		[Column("id_tipo_interaccion")]
		public required long IdTipoInteraccion { get; set; }

		[Required]
		[Column("fecha_interaccion", TypeName = "timestamp with time zone")]
		public required DateTime FechaInteraccion { get; set; }

		[Required]
		[Column("detalle")]
		public required string Detalle { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdCliente))]
		public Cliente? Cliente { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdClienteRelacionado))]
		public Cliente? ClienteRelacionado { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdContactoRelacionado))]
		public Contacto? ContactoRelacionado { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTipoInteraccion))]
		public TipoInteraccion? TipoInteraccion { get; set; }
	}
}
