using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[ExcludeFromCodeCoverage]
	[Table("whatsapp_usuario", Schema = "tanatos")]
	public class WhatsappUsuario {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[Required]
		[Column("sub")]
		public required string Sub { get; set; }

		[Required]
		[Column("whatsapp")]
		public required string Whatsapp { get; set; }

		[Required]
		[Column("id_acceso_validacion")]
		public required long IdAccesoValidacion { get; set; }

		[Column("fecha_validacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaValidacion { get; set; }

		[Required]
		[Column("validado")]
		public required bool Validado { get; set; }

		[Column("hermes_id_mensaje")]
		public string? HermesIdMensaje { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdAccesoValidacion))]
		public Acceso? Acceso { get; set; }
	}
}
