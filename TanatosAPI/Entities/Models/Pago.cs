using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Models {
	[Table("pago", Schema = "tanatos")]
	public class Pago {
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[Required]
		[Column("sub")]
		public required string Sub { get; set; }

		[Required]
		[Column("id_suscripcion")]
		public required long IdSuscripcion { get; set; }

		[Required]
		[Column("monto")]
		public required decimal Monto { get; set; }

		[Required]
		[Column("moneda")]
		public required string Moneda { get; set; } = "CLP";

		[Column("fecha_pago", TypeName = "timestamp with time zone")]
		public DateTime? FechaPago { get; set; }

		[Required]
		[Column("estado")]
		public required short Estado { get; set; }

		[Required]
		[Column("flow_subscription_id")]
		public required string FlowSubscriptionId { get; set; }

		[Required]
		[Column("flow_invoice_id")]
		public required string FlowInvoiceId { get; set; }

		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		public required DateTime FechaCreacion { get; set; }

		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		public DateTime? FechaEliminacion { get; set; }

		[Required]
		[Column("vigencia")]
		public required bool Vigencia { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(IdSuscripcion))]
		public Suscripcion? Suscripcion { get; set; }
	}
}
