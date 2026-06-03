using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("suscripcion", Schema = "tanatos")]
	public class Suscripcion {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("sub")]
		public required string Sub { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_plan")]
		public required long IdPlan { get; set; }

		[UseColumnAttribute]
		[Column("fecha_inicio", TypeName = "timestamp with time zone")]
		public DateTime? FechaInicio { get; set; }

		[UseColumnAttribute]
		[Column("fecha_expiracion", TypeName = "timestamp with time zone")]
		public DateTime? FechaExpiracion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_cancelacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaCancelacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("estado")]
		public required short Estado { get; set; }

		[UseColumnAttribute]
		[Column("flow_customer_id")]
		public string? FlowCustomerId { get; set; }

		[UseColumnAttribute]
		[Column("flow_subscription_id")]
		public string? FlowSubscriptionId { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
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
