using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[ExcludeFromCodeCoverage]
	[Table("acceso_destinatario", Schema = "tanatos")]
	public class AccesoDestinatario {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public required long Id { get; set; }

		[Required]
		[Column("id_acceso")]
		public required long IdAcceso { get; set; }

		[Required]
		[Column("id_destinatario_notificacion")]
		public required long IdDestinatarioNotificacion { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdAcceso))]
		public Acceso? Acceso { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdDestinatarioNotificacion))]
		public DestinatarioNotificacion? DestinatarioNotificacion { get; set; }
	}
}
