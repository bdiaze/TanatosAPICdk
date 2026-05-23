using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Eventing.Reader;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TanatosAPI.Entities.Models {
	[Table("norma_suscrita", Schema = "tanatos")]
	[Comment("Tabla que contiene las normas a las que está suscrita un negocio del usuario.")]
	[Index(nameof(Sub), nameof(IdNegocio))]
	public class NormaSuscrita {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Comment("Identificador de la norma suscrita.")]
		public long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("sub")]
		[Comment("Usuario al que pertenece la norma suscrita.")]
		public required string Sub { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_negocio")]
		[Comment("Identificador del negocio del usuario.")]
		public required long IdNegocio { get; set; }

		[UseColumnAttribute]
		[Column("id_template")]
		[Comment("Identificador del template al que pertenece la norma suscrita.")]
		public long? IdTemplate { get; set; }

		[UseColumnAttribute]
		[Column("id_norma")]
		[Comment("Identificador del template norma al que pertenece la norma suscrita.")]
		public long? IdNorma { get; set; }

		[UseColumnAttribute]
		[Column("nombre")]
		[Comment("Nombre de la norma.")]
		public string? Nombre { get; set; }

		[UseColumnAttribute]
		[Column("descripcion")]
		[Comment("Descripcion de la norma.")]
		public string? Descripcion { get; set; }

		[UseColumnAttribute]
		[Column("id_tipo_periodicidad")]
		[Comment("Identificador del tipo de periodicidad asociado a la norma.")]
		public long? IdTipoPeriodicidad { get; set; }

		[UseColumnAttribute]
		[Column("multa")]
		[Comment("Multa de no cumplir con la norma.")]
		public string? Multa { get; set; }

		[UseColumnAttribute]
		[Column("id_categoria_norma")]
		[Comment("Identificador de la categoría a la que pertenece la norma.")]
		public long? IdCategoriaNorma { get; set; }

        [UseColumnAttribute]
        [Column("id_cargo")]
        [Comment("Identificador del cargo responsable de la norma.")]
        public long? IdCargo { get; set; }

        [UseColumnAttribute]
		[Column("orden_visual")]
		[Comment("Orden en que se deben presentar las normas.")]
		public long? OrdenVisual { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("editable")]
		[Comment("Indicador de si es editable la norma.")]
		public required bool Editable { get; set; }

		[UseColumnAttribute]
		[Column("fecha_activacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se activó el cumplimiento de la norma.")]
		public DateTime? FechaActivacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_desactivacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se desactivó el cumplimiento de la norma.")]
		public DateTime? FechaDesactivacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("activado")]
		[Comment("Estado de activación de la norma.")]
		public required bool Activado { get; set; }

		[UseColumnAttribute]
		[Column("procesos_notificaciones", TypeName = "jsonb")]
		[Comment("Procesos de notificaciones asociados a la norma suscrita.")]
		public List<Dictionary<string, JsonElement>>? ProcesosNotificaciones { get; set; }

		[UseColumnAttribute]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se creó la norma.")]
		public DateTime? FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se eliminó la norma.")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia de la norma.")]
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
