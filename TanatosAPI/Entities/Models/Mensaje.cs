using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("mensaje", Schema = "tanatos")]
	[Comment("Tabla que contiene los mensajes ingresados por formulario de contacto.")]
	[Index(nameof(Sub))]
	[Index(nameof(Correo))]
	[Index(nameof(FechaCreacion))]
	public class Mensaje {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Comment("Identificador de la notificación asociada a una norma suscrita.")]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Column("sub")]
		[Comment("Usuario que ingresó el mensaje.")]
		public string? Sub { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		[Comment("Nombre del usuario que ingresó el mensaje.")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("correo")]
		[Comment("Correo electrónico del usuario que ingresó el mensaje.")]
		public required string Correo { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("contenido")]
		[Comment("Contenido del mensaje.")]
		public required string Contenido { get; set; }

		[UseColumnAttribute]
		[Column("hermes_id_mensaje")]
		[Comment("ID del mensaje en Hermes.")]
		public string? HermesIdMensaje { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se creó el mensaje.")]
		public required DateTime FechaCreacion { get; set; }
	}
}
