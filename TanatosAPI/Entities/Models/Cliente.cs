using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[ExcludeFromCodeCoverage]
	[Table("cliente", Schema = "tanatos")]
	public class Cliente {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[Required]
		[Column("sub")]
		public required string Sub { get; set; }

		[Required]
		[Column("id_negocio")]
		public required long IdNegocio { get; set; }

		[Required]
		[Column("tipo_persona")]
		// 1 = Persona Natural, 2 = Persona Jurídica
		public required short TipoPersona { get; set; }

		[Required]
		[Column("tipo_cliente")]
		// 1 = Cliente, 2 = Subcliente
		public required short TipoCliente { get; set; }

		[Column("rut")]
		public string? Rut { get; set; }

		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[Column("id_cliente_padre")]
		public long? IdClientePadre { get; set; }

		[Column("observacion")]
		public string? Observacion { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdNegocio))]
		public Negocio? Negocio { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdClientePadre))]
		public Cliente? ClientePadre { get; set; }

		[JsonIgnore]
		public List<Cliente>? ClientesHijos { get; set; }

		[JsonIgnore]
		public List<Contacto>? Contactos { get; set; }

		[JsonIgnore]
		public List<Interaccion>? Interacciones { get; set; }

		[JsonIgnore]
		public List<Tarea>? Tareas { get; set; }

		[JsonIgnore]
		public List<Interaccion>? InteraccionesRelacionadas { get; set; }
	}
}
