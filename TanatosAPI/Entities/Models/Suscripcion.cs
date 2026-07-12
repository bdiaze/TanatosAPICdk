using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
    [ExcludeFromCodeCoverage]
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

		[Column("fecha_proximo_cobro", TypeName = "timestamp with time zone")]
		public DateTime? FechaProximoCobro { get; set; }

		[Column("fecha_cancelacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaCancelacion { get; set; }

		[Required]
		[Column("estado")]
		// 1: Activa (Ya se recepcionó algún pago asociado, tiene fecha de expiración)
		// 2: Cancelada (1: Una suscripción activa que fue cancelada - 2: Una suscripción cuyo pago estaba pendiente y fue cancelada previo al primer pago)
		// 3: Expirada (De momento no usada, pero la idea es tener un proceso periodico que tome todas las suscripciones activas, cuya expiración haya pasado)
		// 4: Pago Pendiente (Usuario ya ingreso medio de pago, pero aún no se efectúa el primer cobro, ya sea por delay de plataforma de pago o por ser una suscripción futura)
		// 5: En Creación (Aún no se confirma que usuario haya ingresado medios de pagos)
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
