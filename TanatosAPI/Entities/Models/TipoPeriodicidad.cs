using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("tipo_periodicidad", Schema = "tanatos")]
	public class TipoPeriodicidad {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public required long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Column("descripcion")]
		public string? Descripcion { get; set; }

		[UseColumnAttribute]
		[Column("cron")]
		public string? Cron { get; set; }

        [UseColumnAttribute]
        [Column("delta_dias")]
        public int? DeltaDias { get; set; }

        [UseColumnAttribute]
        [Column("delta_meses")]
        public int? DeltaMeses { get; set; }

        [UseColumnAttribute]
        [Column("delta_annos")]
        public int? DeltaAnnos { get; set; }

        [UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<TemplateNorma>? TemplateNormas { get; set; }

		[JsonIgnore]
		public List<NormaSuscrita>? NormasSuscritas { get; set; }

	}
}
