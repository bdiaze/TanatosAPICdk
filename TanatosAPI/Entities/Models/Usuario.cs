using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TanatosAPI.Entities.Models {
	[Table("usuario", Schema = "tanatos")]
	[Comment("Tabla que contiene la información del usuario.")]
	[Index(nameof(FlowCustomerId), IsUnique = true)]
	public class Usuario {
		[UseColumnAttribute]
		[Key]
		[Column("sub")]
		[Comment("Identificador del usuario.")]
		public required string Sub { get; set; }

		[UseColumnAttribute]
		[Column("flow_customer_id")]
		[Comment("ID del cliente en Flow.")]
		public string? FlowCustomerId { get; set; }

		[UseColumnAttribute]
		[Column("nombre")]
		[Comment("Nombre del usuario.")]
		public string? Nombre { get; set; }

		[UseColumnAttribute]
		[Column("apellido")]
		[Comment("Apellido del usuario.")]
		public string? Apellido { get; set; }

		[UseColumnAttribute]
		[Column("correo_electronico")]
		[Comment("Correo electrónico del usuario.")]
		public string? CorreoElectronico { get; set; }
	}
}
