using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("template_norma_notificacion", Schema = "tanatos")]
	public class TemplateNormaNotificacion {
		[UseColumnAttribute]
		[Required]
		[Column("id_template")]
		public required long IdTemplate { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_norma")]
		public required long IdNorma { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_tipo_unidad_tiempo_antelacion")]
		public required long IdTipoUnidadTiempoAntelacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("cant_antelacion")]
		public required int CantAntelacion { get; set; }

		[JsonIgnore]
		public TemplateNorma? TemplateNorma { get; set; }

		[JsonIgnore]
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
