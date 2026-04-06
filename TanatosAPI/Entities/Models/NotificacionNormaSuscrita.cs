using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("notificacion_norma_suscrita", Schema = "tanatos")]
	[Comment("Tabla que contiene las notificaciones asociados a una norma suscrita.")]
	[Index(nameof(IdNormaSuscrita), nameof(Vigencia))]
	public class NotificacionNormaSuscrita {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Comment("Identificador de la notificación asociada a una norma suscrita.")]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_norma_suscrita")]
		[Comment("Identificador de la norma suscrita.")]
		public required long IdNormaSuscrita { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_tipo_unidad_tiempo_antelacion")]
		[Comment("Identificador del tipo de unidad de tiempo a usar para la notificación.")]
		public required long IdTipoUnidadTiempoAntelacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("cant_antelacion")]
		[Comment("Cantidad de unidades de tiempo a usar para la notificación.")]
		public required int CantAntelacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se creó la notificación asociada.")]
		public DateTime? FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se eliminó la notificación asociada.")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia de la notificación asociada.")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdNormaSuscrita))]
		public NormaSuscrita? NormaSuscrita { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdTipoUnidadTiempoAntelacion))]
		public TipoUnidadTiempo? TipoUnidadTiempo { get; set; }

		public override int GetHashCode() {
			return HashCode.Combine(Id, IdNormaSuscrita, IdTipoUnidadTiempoAntelacion, CantAntelacion);
		}

		public override bool Equals(object? obj) {
			if (obj is not NotificacionNormaSuscrita other) {
				return false;
			}
			return Id == other.Id &&
				   IdNormaSuscrita == other.IdNormaSuscrita &&
				   IdTipoUnidadTiempoAntelacion == other.IdTipoUnidadTiempoAntelacion &&
				   CantAntelacion == other.CantAntelacion;
		}
	}
}
