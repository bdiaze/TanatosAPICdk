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
		[Required]
		[Column("correo_electronico")]
		[Comment("Correo electrónico del cliente.")]
		public required string CorreoElectronico { get; set; }
	}
}
