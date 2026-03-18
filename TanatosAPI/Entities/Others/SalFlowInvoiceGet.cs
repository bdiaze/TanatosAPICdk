using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others {
	public class SalFlowInvoiceGet {
		[JsonPropertyName("id")]
		public long? Id { get; set; }

		[JsonPropertyName("subscriptionId")]
		public string? SubscriptionId { get; set; }

		[JsonPropertyName("customerId")]
		public string? CustomerId { get; set; }

		[JsonPropertyName("created")]
		public string? Created { get; set; }

		[JsonPropertyName("subject")]
		public string? Subject { get; set; }

		[JsonPropertyName("currency")]
		public string? Currency { get; set; }

		[JsonPropertyName("amount")]
		public string? Amount { get; set; }

		[JsonPropertyName("period_start")]
		public string? PeriodStart { get; set; }

		[JsonPropertyName("period_end")]
		public string? PeriodEnd { get; set; }

		[JsonPropertyName("attemp_count")]
		public short? AttempCount { get; set; }

		[JsonPropertyName("attemped")]
		public short? Attemped { get; set; }

		[JsonPropertyName("next_attemp_date")]
		public string? NextAttempDate { get; set; }

		[JsonPropertyName("due_date")]
		public string? DueDate { get; set; }

		[JsonPropertyName("status")]
		/* 0: Impago - 1: Pagado - 2: Anulado */
		public short? Status { get; set; }

		[JsonPropertyName("error")]
		/* 0: Sin error - 1: Con error */
		public short? Error { get; set; }

		[JsonPropertyName("errorDate")]
		public string? ErrorDate { get; set; }

		[JsonPropertyName("errorDescription")]
		public string? ErrorDescription { get; set; }

		[JsonPropertyName("items")]
		public SalFlowInvoiceItem[]? Items { get; set; }

		[JsonPropertyName("payment")]
		public SalFlowPaymentGetStatus? Payment { get; set; }

		[JsonPropertyName("outsidePayment")]
		public SalFlowInvoiceOutsidePayment? OutsidePayment { get; set; }

		[JsonPropertyName("paymentLink")]
		public string? PaymentLink { get; set; }

		[JsonPropertyName("chargeAttemps")]
		public SalFlowInvoiceChargeAttemp[]? ChargeAttemps { get; set; }
	}

	public class SalFlowInvoiceItem {
		[JsonPropertyName("id")]
		public long? Id { get; set; }

		[JsonPropertyName("subject")]
		public string? Subject { get; set; }

		[JsonPropertyName("type")]
		/* 1: Cargo por plan - 2: Descuento - 3: Item pendiente - 9: Otros */
		public short? Type { get; set; }

		[JsonPropertyName("currency")]
		public string? Currency { get; set; }

		[JsonPropertyName("amount")]
		public string? Amount { get; set; }
	}

	public class SalFlowInvoiceOutsidePayment {
		[JsonPropertyName("date")]
		public string? Date { get; set; }

		[JsonPropertyName("comment")]
		public string? Comment { get; set; }
	}

	public class SalFlowInvoiceChargeAttemp {
		[JsonPropertyName("id")]
		public long? Id { get; set; }

		[JsonPropertyName("date")]
		public string? Date { get; set; }

		[JsonPropertyName("customerId")]
		public string? CustomerId { get; set; }

		[JsonPropertyName("invoiceId")]
		public string? InvoiceId { get; set; }

		[JsonPropertyName("commerceOrder")]
		public string? CommerceOrder { get; set; }

		[JsonPropertyName("currency")]
		public string? Currency { get; set; }

		[JsonPropertyName("amount")]
		public string? Amount { get; set; }

		[JsonPropertyName("errorCode")]
		public string? ErrorCode { get; set; }

		[JsonPropertyName("errorDescription")]
		public string? ErrorDescription { get; set; }
	}
}
