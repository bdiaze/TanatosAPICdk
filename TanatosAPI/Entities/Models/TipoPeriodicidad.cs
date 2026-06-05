using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
    [Table("tipo_periodicidad", Schema = "tanatos")]
	public class TipoPeriodicidad {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public required long Id { get; set; }

		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[Column("descripcion")]
		public string? Descripcion { get; set; }

		[Column("cron")]
		public string? Cron { get; set; }

        [Column("delta_dias")]
        public int? DeltaDias { get; set; }

        [Column("delta_meses")]
        public int? DeltaMeses { get; set; }

        [Column("delta_annos")]
        public int? DeltaAnnos { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<TemplateNorma>? TemplateNormas { get; set; }

		[JsonIgnore]
		public List<NormaSuscrita>? NormasSuscritas { get; set; }

	}
}
