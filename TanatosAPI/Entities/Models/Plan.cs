using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("plan", Schema = "tanatos")]
	public class Plan {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long Id { get; set; }

		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[Required]
		[Column("precio")]
		public required decimal Precio { get; set; }

		[Required]
		[Column("duracion_meses")]
		public required int DuracionMeses { get; set; }

		[Required]
		[Column("suscripcion_unica")]
		[DefaultValue(false)]
		public required bool SuscripcionUnica { get; set; }

		[Column("flow_plan_id")]
		public string? FlowPlanId { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<Suscripcion>? Suscripciones { get; set; }
	}
}
