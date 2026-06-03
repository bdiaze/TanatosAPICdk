using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("empleado", Schema = "tanatos")]
	public class Empleado {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("sub")]
		public required string Sub { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_negocio")]
		public required long IdNegocio { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Column("id_cargo")]
		public long? IdCargo { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdNegocio))]
		public Negocio? Negocio { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdCargo))]
		public Cargo? Cargo { get; set; }

		[JsonIgnore]
		public List<DestinatarioNotificacion>? DestinatariosNotificaciones { get; set; }
	}
}
