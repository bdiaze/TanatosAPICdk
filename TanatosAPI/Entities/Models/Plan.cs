using Dapper;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("plan", Schema = "tanatos")]
	public class Plan {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("precio")]
		public required decimal Precio { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("duracion_meses")]
		public required int DuracionMeses { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("suscripcion_unica")]
		[DefaultValue(false)]
		public required bool SuscripcionUnica { get; set; }

		[UseColumnAttribute]
		[Column("flow_plan_id")]
		public string? FlowPlanId { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<Suscripcion>? Suscripciones { get; set; }
	}
}
