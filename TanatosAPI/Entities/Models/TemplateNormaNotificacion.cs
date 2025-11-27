using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("template_norma_notificacion", Schema = "tanatos")]
	[Comment("Tabla que contiene las notificaciones asociadas a una template norma.")]
	[PrimaryKey(nameof(IdTemplateNorma), nameof(IdTipoUnidadTiempoAntelacion))]
	[Index(nameof(IdTipoUnidadTiempoAntelacion))]
	public class TemplateNormaNotificacion {
		[Required]
		[Column("id_template_norma")]
		[Comment("Identificador de la template norma.")]
		public required long IdTemplateNorma { get; set; }

		[Required]
		[Column("id_tipo_unidad_tiempo_antelacion")]
		[Comment("Identificador del tipo de unidad de tiempo a usar para la notificación.")]
		public required long IdTipoUnidadTiempoAntelacion { get; set; }

		[Required]
		[Column("cant_antelacion")]
		[Comment("Cantidad de unidades de tiempo a usar para la notificación.")]
		public required int CantAntelacion { get; set; }

		[ForeignKey(nameof(IdTemplateNorma))]
		public TemplateNorma? TemplateNorma { get; set; }

		[ForeignKey(nameof(IdTipoUnidadTiempoAntelacion))]
		public TipoUnidadTiempo? TipoUnidadTiempoAntelacion { get; set; }
	}
}
