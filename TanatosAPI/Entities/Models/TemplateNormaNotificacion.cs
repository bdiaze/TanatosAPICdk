using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("template_norma_notificacion", Schema = "tanatos")]
	[Comment("Tabla que contiene las notificaciones asociadas a una template norma.")]
	[PrimaryKey(nameof(IdTemplate), nameof(IdNorma), nameof(IdTipoUnidadTiempoAntelacion), nameof(CantAntelacion))]
	[Index(nameof(IdTipoUnidadTiempoAntelacion))]
	public class TemplateNormaNotificacion {
		[Required]
		[Column("id_template")]
		[Comment("Identificador del template al que pertenece la norma.")]
		public required long IdTemplate { get; set; }

		[Required]
		[Column("id_norma")]
		[Comment("Identificador de la norma asociada al template.")]
		public required long IdNorma { get; set; }

		[Required]
		[Column("id_tipo_unidad_tiempo_antelacion")]
		[Comment("Identificador del tipo de unidad de tiempo a usar para la notificación.")]
		public required long IdTipoUnidadTiempoAntelacion { get; set; }

		[Required]
		[Column("cant_antelacion")]
		[Comment("Cantidad de unidades de tiempo a usar para la notificación.")]
		public required int CantAntelacion { get; set; }

		public TemplateNorma? TemplateNorma { get; set; }

		[ForeignKey(nameof(IdTipoUnidadTiempoAntelacion))]
		public TipoUnidadTiempo? TipoUnidadTiempoAntelacion { get; set; }

		public override int GetHashCode() {
			return HashCode.Combine(IdTemplate, IdNorma, IdTipoUnidadTiempoAntelacion, CantAntelacion);
		}

		public override bool Equals(object? obj) {
			if (obj is not TemplateNormaNotificacion other) {
				return false;
			}

			return IdTemplate == other.IdTemplate && 
					IdNorma == other.IdNorma &&
					IdTipoUnidadTiempoAntelacion == other.IdTipoUnidadTiempoAntelacion &&
					CantAntelacion == other.CantAntelacion;
		}
	}
}
