using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
    [Table("usuario", Schema = "tanatos")]
	public class Usuario {
		[Key]
		[Column("sub")]
		public required string Sub { get; set; }

		[Column("flow_customer_id")]
		public string? FlowCustomerId { get; set; }

		[Column("nombre")]
		public string? Nombre { get; set; }

		[Column("apellido")]
		public string? Apellido { get; set; }

		[Column("correo_electronico")]
		public string? CorreoElectronico { get; set; }
	}
}
