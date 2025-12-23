using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("inscripcion_template", Schema = "tanatos")]
	[Comment("Tabla que contiene los templates a los que un usuario está inscrito.")]
	[PrimaryKey(nameof(Sub), nameof(IdNegocio), nameof(IdTemplate))]
	[Index(nameof(IdTemplate))]
	public class InscripcionTemplate {
		[UseColumnAttribute]
		[Required]
		[Column("sub")]
		[Comment("Usuario al que está asociada la inscripción.")]
		public required string Sub { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_negocio")]
		[Comment("Identificador del negocio del usuario.")]
		public required long IdNegocio { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_template")]
		[Comment("Identificador del template al que está inscrito el usuario.")]
		public required long IdTemplate { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_activacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se activa la inscripción.")]
		public required DateTime FechaActivacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_desactivacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se desactiva la inscripción.")]
		public DateTime? FechaDesactivacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia de la inscripción.")]
		public required bool Vigencia { get; set; }

		[ForeignKey(nameof(IdTemplate))]
		public Template? Template { get; set; }

		[ForeignKey(nameof(IdNegocio))]
		public Negocio? Negocio { get; set; }
	}
}
