using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("negocio", Schema = "tanatos")]
	[Comment("Tabla que contiene los negocios de un usuario.")]
	[Index(nameof(Sub), nameof(Nombre), IsUnique = true)]
	public class Negocio {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Comment("Identificador del negocio.")]
		public long Id { get; set; }

		[Required]
		[Column("sub")]
		[Comment("Usuario al que pertenece el negocio.")]
		public required string Sub { get; set; }

		[Required]
		[Column("nombre")]
		[Comment("Nombre del negocio.")]
		public required string Nombre { get; set; }

		[Column("direccion")]
		[Comment("Dirección del negocio.")]
		public string? Direccion { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se creó el negocio.")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se eliminó el negocio.")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del negocio.")]
		public required bool Vigencia { get; set; }

		public List<DestinatarioNotificacion>? DestinatariosNotificaciones { get; set; }

		public List<NormaSuscrita>? NormasSuscritas { get; set; }
	}
}
