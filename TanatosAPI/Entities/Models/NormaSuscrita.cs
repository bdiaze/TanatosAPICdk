using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("norma_suscrita", Schema = "tanatos")]
	public class NormaSuscrita {
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
		[Column("id_template")]
		public long? IdTemplate { get; set; }

		[UseColumnAttribute]
		[Column("id_norma")]
		public long? IdNorma { get; set; }

		[UseColumnAttribute]
		[Column("nombre")]
		public string? Nombre { get; set; }

		[UseColumnAttribute]
		[Column("descripcion")]
		public string? Descripcion { get; set; }

		[UseColumnAttribute]
		[Column("id_tipo_periodicidad")]
		public long? IdTipoPeriodicidad { get; set; }

		[UseColumnAttribute]
		[Column("multa")]
		public string? Multa { get; set; }

		[UseColumnAttribute]
		[Column("id_categoria_norma")]
		public long? IdCategoriaNorma { get; set; }

        [UseColumnAttribute]
        [Column("id_cargo")]
        public long? IdCargo { get; set; }

        [UseColumnAttribute]
		[Column("orden_visual")]
		public long? OrdenVisual { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("editable")]
		public required bool Editable { get; set; }

		[UseColumnAttribute]
		[Column("fecha_activacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaActivacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_desactivacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaDesactivacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("activado")]
		public required bool Activado { get; set; }

		[UseColumnAttribute]
		[Column("procesos_notificaciones", TypeName = "jsonb")]
		public List<Dictionary<string, JsonElement>>? ProcesosNotificaciones { get; set; }

		[UseColumnAttribute]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
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
