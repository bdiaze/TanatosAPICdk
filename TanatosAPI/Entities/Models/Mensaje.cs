using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("mensaje", Schema = "tanatos")]
	public class Mensaje {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Column("sub")]
		public string? Sub { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("correo")]
		public required string Correo { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("contenido")]
		public required string Contenido { get; set; }

		[UseColumnAttribute]
		[Column("hermes_id_mensaje")]
		public string? HermesIdMensaje { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }
	}
}
