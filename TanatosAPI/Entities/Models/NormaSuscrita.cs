using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
    [Table("norma_suscrita", Schema = "tanatos")]
	public class NormaSuscrita {
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

		[Column("id_template")]
		public long? IdTemplate { get; set; }

		[Column("id_norma")]
		public long? IdNorma { get; set; }

		[Column("nombre")]
		public string? Nombre { get; set; }

		[Column("descripcion")]
		public string? Descripcion { get; set; }

		[Column("id_tipo_periodicidad")]
		public long? IdTipoPeriodicidad { get; set; }

		[Column("multa")]
		public string? Multa { get; set; }

		[Column("id_categoria_norma")]
		public long? IdCategoriaNorma { get; set; }

        [Column("id_cargo")]
        public long? IdCargo { get; set; }

		[Column("orden_visual")]
		public long? OrdenVisual { get; set; }

		[Required]
		[Column("editable")]
		public required bool Editable { get; set; }

		[Column("fecha_activacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaActivacion { get; set; }

		[Column("fecha_desactivacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaDesactivacion { get; set; }

		[Required]
		[Column("activado")]
		public required bool Activado { get; set; }

		[Column("procesos_notificaciones", TypeName = "jsonb")]
		public List<Dictionary<string, JsonElement>>? ProcesosNotificaciones { get; set; }

		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTipoPeriodicidad))]
		public TipoPeriodicidad? TipoPeriodicidad { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdCategoriaNorma))]
		public CategoriaNorma? CategoriaNorma { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdNegocio))]
		public Negocio? Negocio { get; set; }

		[JsonIgnore]
		public TemplateNorma? TemplateNorma { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(IdCargo))]
        public Cargo? Cargo { get; set; }

        [JsonIgnore]
		public List<FiscalizadorNormaSuscrita>? FiscalizadoresNormaSuscrita { get; set; }

		[JsonIgnore]
		public List<NotificacionNormaSuscrita>? NotificacionesNormaSuscrita { get; set; }

		[JsonIgnore]
		public List<HistorialNormaSuscrita>? HistorialesNormaSuscrita { get; set; }
	}
}
