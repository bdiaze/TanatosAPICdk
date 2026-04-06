using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("tipo_periodicidad", Schema = "tanatos")]
	[Comment("Tabla que contiene los tipos de periodicidad.")]
	public class TipoPeriodicidad {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Comment("Identificador del tipo de periodicidad.")]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		[Comment("Nombre del tipo de periodicidad.")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Column("descripcion")]
		[Comment("Descripción del tipo de periodicidad.")]
		public string? Descripcion { get; set; }

		[UseColumnAttribute]
		[Column("cron")]
		[Comment("Cron del tipo de periodicidad.")]
		public string? Cron { get; set; }

        [UseColumnAttribute]
        [Column("delta_dias")]
        [Comment("Delta en días de la periodicidad.")]
        public int? DeltaDias { get; set; }

        [UseColumnAttribute]
        [Column("delta_meses")]
        [Comment("Delta en meses de la periodicidad.")]
        public int? DeltaMeses { get; set; }

        [UseColumnAttribute]
        [Column("delta_annos")]
        [Comment("Delta en años de la periodicidad.")]
        public int? DeltaAnnos { get; set; }

        [UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del tipo de periodicidad.")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<TemplateNorma>? TemplateNormas { get; set; }

		[JsonIgnore]
		public List<NormaSuscrita>? NormasSuscritas { get; set; }

	}
}
