using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
    [Table("mensaje", Schema = "tanatos")]
	public class Mensaje {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public required long Id { get; set; }

		[Column("sub")]
		public string? Sub { get; set; }

		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[Required]
		[Column("correo")]
		public required string Correo { get; set; }

		[Required]
		[Column("contenido")]
		public required string Contenido { get; set; }

		[Column("hermes_id_mensaje")]
		public string? HermesIdMensaje { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }
	}
}
