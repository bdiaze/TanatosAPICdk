using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others {
	public class SalFlowPaymentGetStatus {

		[JsonPropertyName("flowOrder")]
		public long? FlowOrder { get; set; }

		[JsonPropertyName("commerceOrder")]
		public string? CommerceOrder { get; set; }

		[JsonPropertyName("requestDate")]
		public string? RequestDate { get; set; }

		[JsonPropertyName("status")]
		/* 1: Pendiente de pago - 2: Pagada - 3: Rechazada - 4: Anulada */
		public short? Status { get; set; }

		[JsonPropertyName("subject")]
		public string? Subject { get; set; }

		[JsonPropertyName("currency")]
		public string? Currency { get; set; }

		[JsonPropertyName("amount")]
		public decimal? Amount { get; set; }

		[JsonPropertyName("payer")]
		public string? Payer { get; set; }

		[JsonPropertyName("optional")]
		public string? Optional { get; set; }

		[JsonPropertyName("pending_info")]
		public SalFlowPaymentPendingInfo? PendingInfo { get; set; }

		[JsonPropertyName("paymentData")]
		public SalFlowPaymentData? PaymentData { get; set; }

		[JsonPropertyName("merchantId")]
		public string? MerchantId { get; set; }
	}

	public class SalFlowPaymentPendingInfo {
		[JsonPropertyName("media")]
		public string? Media { get; set; }

		[JsonPropertyName("date")]
		public string? Date { get; set; }
	}

	public class SalFlowPaymentData {
		[JsonPropertyName("date")]
		public string? Date { get; set; }

		[JsonPropertyName("media")]
		public string? Media { get; set; }

		[JsonPropertyName("conversionDate")]
		public string? ConversionDate { get; set; }

		[JsonPropertyName("conversionRate")]
		public decimal? ConversionRate { get; set; }

		[JsonPropertyName("amount")]
		public decimal? Amount { get; set; }

		[JsonPropertyName("currency")]
		public string? Currency { get; set; }

		[JsonPropertyName("fee")]
		public decimal? Fee { get; set; }

		[JsonPropertyName("balance")]
		public decimal? Balance { get; set; }

		[JsonPropertyName("transferDate")]
		public string? TransferDate { get; set; }
	}
}
