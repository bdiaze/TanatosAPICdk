using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [Table("destinatario_notificacion", Schema = "tanatos")]
    public class DestinatarioNotificacion {
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
		[Column("id_empleado")]
		public long? IdEmpleado { get; set; }

		[UseColumnAttribute]
		[Required]
        [Column("id_tipo_receptor")]
        public required long IdTipoReceptor { get; set; }

		[UseColumnAttribute]
		[Column("alias")]
		public string? Alias { get; set; }

		[UseColumnAttribute]
		[Required]
        [Column("destino")]
        public required string Destino { get; set; }

		[UseColumnAttribute]
		[Required]
        [Column("codigo_validacion")]
        public required string CodigoValidacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_caducidad_codigo_validacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCaducidadCodigoValidacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_validacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaValidacion { get; set; }

		[UseColumnAttribute]
		[Required]
        [Column("validado")]
        public required bool Validado { get; set; }

		[UseColumnAttribute]
		[Column("hermes_id_mensaje")]
		public string? HermesIdMensaje { get; set; }

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
		[ForeignKey(nameof(IdTipoReceptor))]
        public TipoReceptorNotificacion? TipoReceptorNotificacion { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdNegocio))]
		public Negocio? Negocio { get; set; }

		[JsonIgnore]
		public List<HistorialNotificacion>? HistorialNotificaciones { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdEmpleado))]
		public Empleado? Empleado { get; set; }
	}
}
