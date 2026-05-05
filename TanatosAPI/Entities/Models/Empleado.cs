using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("empleado", Schema = "tanatos")]
	[Comment("Tabla que contiene los empleados asociados a un negocio.")]
	[Index(nameof(Sub), nameof(IdNegocio))]
	public class Empleado {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Comment("Identificador del empleado.")]
		public long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("sub")]
		[Comment("Usuario al que pertenece el empleado.")]
		public required string Sub { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_negocio")]
		[Comment("Identificador del negocio del usuario.")]
		public required long IdNegocio { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		[Comment("Nombre del empleado.")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Column("id_cargo")]
		[Comment("Identificador del cargo del empleado.")]
		public long? IdCargo { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se creó el empleado.")]
		public required DateTime FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se eliminó el empleado.")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del empleado.")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdNegocio))]
		public Negocio? Negocio { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdCargo))]
		public Cargo? Cargo { get; set; }
	}
}
