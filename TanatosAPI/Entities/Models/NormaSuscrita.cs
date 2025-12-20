using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Eventing.Reader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TanatosAPI.Entities.Models {
	[Table("norma_suscrita", Schema = "tanatos")]
	[Comment("Tabla que contiene las normas a las que está suscrita un negocio del usuario.")]
	[Index(nameof(Sub), nameof(IdNegocio))]
	public class NormaSuscrita {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Comment("Identificador de la norma suscrita.")]
		public long Id { get; set; }

		[Required]
		[Column("sub")]
		[Comment("Usuario al que pertenece la norma suscrita.")]
		public required string Sub { get; set; }

		[Required]
		[Column("id_negocio")]
		[Comment("Identificador del negocio del usuario.")]
		public required long IdNegocio { get; set; }

		[Column("id_template")]
		[Comment("Identificador del template al que pertenece la norma suscrita.")]
		public long? IdTemplate { get; set; }

		[Column("id_norma")]
		[Comment("Identificador del template norma al que pertenece la norma suscrita.")]
		public long? IdNorma { get; set; }

		[Column("nombre")]
		[Comment("Nombre de la norma.")]
		public string? Nombre { get; set; }

		[Column("descripcion")]
		[Comment("Descripcion de la norma.")]
		public string? Descripcion { get; set; }

		[Column("id_tipo_periodicidad")]
		[Comment("Identificador del tipo de periodicidad asociado a la norma.")]
		public long? IdTipoPeriodicidad { get; set; }

		[Column("multa")]
		[Comment("Multa de no cumplir con la norma.")]
		public string? Multa { get; set; }

		[Column("id_categoria_norma")]
		[Comment("Identificador de la categoría a la que pertenece la norma.")]
		public long? IdCategoriaNorma { get; set; }

		[Column("orden_visual")]
		[Comment("Orden en que se deben presentar las normas.")]
		public long? OrdenVisual { get; set; }

		[Required]
		[Column("editable")]
		[Comment("Indicador de si es editable la norma.")]
		public required bool Editable { get; set; }

		[Column("fecha_activacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se activó el cumplimiento de la norma.")]
		public DateTime? FechaActivacion { get; set; }

		[Column("fecha_desactivacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se desactivó el cumplimiento de la norma.")]
		public DateTime? FechaDesactivacion { get; set; }

		[Required]
		[Column("activado")]
		[Comment("Estado de activación de la norma.")]
		public required bool Activado { get; set; }

		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se creó la norma.")]
		public DateTime? FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se eliminó la norma.")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		[Comment("Vigencia de la norma.")]
		public required bool Vigencia { get; set; }

		[ForeignKey(nameof(IdTipoPeriodicidad))]
		public TipoPeriodicidad? TipoPeriodicidad { get; set; }

		[ForeignKey(nameof(IdCategoriaNorma))]
		public CategoriaNorma? CategoriaNorma { get; set; }

		[ForeignKey(nameof(IdNegocio))]
		public Negocio? Negocio { get; set; }

		public TemplateNorma? TemplateNorma { get; set; }

		public List<FiscalizadorNormaSuscrita>? FiscalizadoresNormaSuscrita { get; set; }

		public List<NotificacionNormaSuscrita>? NotificacionesNormaSuscrita { get; set; }

		public List<HistorialNormaSuscrita>? HistorialesNormaSuscrita { get; set; }
	}
}
