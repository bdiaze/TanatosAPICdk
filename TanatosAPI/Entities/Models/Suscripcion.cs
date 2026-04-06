using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TanatosAPI.Entities.Models {
	[Table("suscripcion", Schema = "tanatos")]
	[Comment("Tabla que contiene las suscripciones de los usuarios.")]
	[Index(nameof(Sub))]
	[Index(nameof(FlowSubscriptionId), IsUnique = true)]
	public class Suscripcion {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Comment("Identificador de la suscripción.")]
		public long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("sub")]
		[Comment("Usuario al que pertenece la suscripción.")]
		public required string Sub { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_plan")]
		[Comment("Identificador del plan al que el usuario está suscrito.")]
		public required long IdPlan { get; set; }

		[UseColumnAttribute]
		[Column("fecha_inicio", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se inició la suscripción.")]
		public DateTime? FechaInicio { get; set; }

		[UseColumnAttribute]
		[Column("fecha_expiracion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que expira la suscripción.")]
		public DateTime? FechaExpiracion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_cancelacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se cancela la suscripción.")]
		public DateTime? FechaCancelacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("estado")]
		[Comment("Estado de la suscripción. 1: Activa - 2: Cancelada - 3: Expirada - 4: Pago Pendiente.")]
		public required short Estado { get; set; }

		[UseColumnAttribute]
		[Column("flow_customer_id")]
		[Comment("ID del cliente en la plataforma Flow.")]
		public string? FlowCustomerId { get; set; }

		[UseColumnAttribute]
		[Column("flow_subscription_id")]
		[Comment("ID de la suscripción en la plataforma Flow.")]
		public string? FlowSubscriptionId { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se creó la suscripción.")]
		public required DateTime FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se eliminó la suscripción.")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia de la suscripción.")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdPlan))]
		public Plan? Plan { get; set; }

		[JsonIgnore]
		public List<Pago>? Pagos { get; set; }
	}
}
