using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("suscripcion", Schema = "tanatos")]
	public class Suscripcion {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[Required]
		[Column("sub")]
		public required string Sub { get; set; }

		[Required]
		[Column("id_plan")]
		public required long IdPlan { get; set; }

		[Column("fecha_inicio", TypeName = "timestamp with time zone")]
		public DateTime? FechaInicio { get; set; }

		[Column("fecha_expiracion", TypeName = "timestamp with time zone")]
		public DateTime? FechaExpiracion { get; set; }

		[Column("fecha_cancelacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaCancelacion { get; set; }

		[Required]
		[Column("estado")]
		public required short Estado { get; set; }

		[Column("flow_customer_id")]
		public string? FlowCustomerId { get; set; }

		[Column("flow_subscription_id")]
		public string? FlowSubscriptionId { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdPlan))]
		public Plan? Plan { get; set; }

		[JsonIgnore]
		public List<Pago>? Pagos { get; set; }
	}
}
