using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
    [Table("negocio", Schema = "tanatos")]
	public class Negocio {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[Required]
		[Column("sub")]
		public required string Sub { get; set; }

		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[Column("direccion")]
		public string? Direccion { get; set; }

		[Column("id_tipo_actividad")]
		public long? IdTipoActividad { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<DestinatarioNotificacion>? DestinatariosNotificaciones { get; set; }

		[JsonIgnore]
		public List<NormaSuscrita>? NormasSuscritas { get; set; }

		[JsonIgnore]
		public List<InscripcionTemplate>? InscripcionesTemplates { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTipoActividad))]
		public TipoActividad? TipoActividad { get; set; }

        [JsonIgnore]
        public List<Cargo>? Cargos { get; set; }

		[JsonIgnore]
		public List<Empleado>? Empleados { get; set; }

	}
}
