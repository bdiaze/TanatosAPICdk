using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[ExcludeFromCodeCoverage]
	[Table("contacto", Schema = "tanatos")]
	public class Contacto {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[Required]
		[Column("id_cliente")]
		public required long IdCliente { get; set; }

		[Required]
		[Column("id_tipo_contacto")]
		public required long IdTipoContacto { get; set; }

		[Required]
		[Column("valor")]
		public required string Valor { get; set; }

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
		[ForeignKey(nameof(IdTipoContacto))]
		public TipoContacto? TipoContacto { get; set; }

		[JsonIgnore]
		public List<Interaccion>? InteraccionesRelacionadas { get; set; }
	}
}
