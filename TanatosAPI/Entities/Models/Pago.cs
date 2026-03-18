using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TanatosAPI.Entities.Models {
	[Table("pago", Schema = "tanatos")]
	[Comment("Tabla que contiene los pagos de los usuarios.")]
	[Index(nameof(Sub))]
	[Index(nameof(FlowSubscriptionId), nameof(FlowInvoiceId), IsUnique = true)]
	public class Pago {
		[UseColumnAttribute]
		[Required]
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Comment("Identificador del pago.")]
		public long Id { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("sub")]
		[Comment("Usuario al que pertenece el pago.")]
		public required string Sub { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("id_suscripcion")]
		[Comment("Identificador de la suscripción a la que pertenece el pago.")]
		public required long IdSuscripcion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("monto")]
		[Comment("Monto del pago efectuado.")]
		public required decimal Monto { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("moneda")]
		[Comment("Moneda en que se efectuó el pago.")]
		public required string Moneda { get; set; } = "CLP";

		[UseColumnAttribute]
		[Column("fecha_pago", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se efectuó el pago.")]
		public DateTime? FechaPago { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("estado")]
		[Comment("Estado del pago. 0: Pendiente - 1: Pagado - 2: Fallido - 3: Reembolsado.")]
		public required short Estado { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("flow_subscription_id")]
		[Comment("ID de la suscripción en la plataforma Flow.")]
		public required string FlowSubscriptionId { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("flow_invoice_id")]
		[Comment("ID del invoice en la plataforma Flow.")]
		public required string FlowInvoiceId { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("fecha_creacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se creó el pago.")]
		public required DateTime FechaCreacion { get; set; }

		[UseColumnAttribute]
		[Column("fecha_eliminacion", TypeName = "timestamp with time zone")]
		[Comment("Fecha en que se eliminó el pago.")]
		public DateTime? FechaEliminacion { get; set; }

		[UseColumnAttribute]
		[Required]
		[Column("vigencia")]
		[Comment("Vigencia del pago.")]
		public required bool Vigencia { get; set; }

		[ForeignKey(nameof(IdSuscripcion))]
		public Suscripcion? Suscripcion { get; set; }
	}
}
