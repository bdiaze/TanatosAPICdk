using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("usuario", Schema = "tanatos")]
	public class Usuario {
		[UseColumnAttribute]
		[Key]
		[Column("sub")]
		public required string Sub { get; set; }

		[UseColumnAttribute]
		[Column("flow_customer_id")]
		public string? FlowCustomerId { get; set; }

		[UseColumnAttribute]
		[Column("nombre")]
		public string? Nombre { get; set; }

		[UseColumnAttribute]
		[Column("apellido")]
		public string? Apellido { get; set; }

		[UseColumnAttribute]
		[Column("correo_electronico")]
		public string? CorreoElectronico { get; set; }
	}
}
