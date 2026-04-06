using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("plan", Schema = "tanatos")]
	[Comment("Tabla que contiene los planes de suscripción.")]
	[Index(nameof(FlowPlanId), IsUnique = true)]
	public class Plan {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Comment("Identificador del plan.")]
		public long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("nombre")]
		[Comment("Nombre del plan.")]
		public required string Nombre { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("precio")]
		[Comment("Precio del plan.")]
		public required decimal Precio { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("duracion_meses")]
		[Comment("Duración del plan en meses.")]
		public required int DuracionMeses { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("suscripcion_unica")]
		[Comment("Indicador de si el plan solo permite una suscripción única por usuario.")]
		[DefaultValue(false)]
		public required bool SuscripcionUnica { get; set; }

		[UseColumnAttribute]
		[Column("flow_plan_id")]
		[Comment("ID del plan en la plataforma Flow.")]
		public string? FlowPlanId { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del plan.")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		public List<Suscripcion>? Suscripciones { get; set; }
	}
}
