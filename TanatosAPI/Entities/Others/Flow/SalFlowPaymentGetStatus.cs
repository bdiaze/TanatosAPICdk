using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others.Flow {
    [ExcludeFromCodeCoverage]
    public class SalFlowPaymentGetStatus : ISalFlow {

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
		public string? Amount { get; set; }

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

    [ExcludeFromCodeCoverage]
    public class SalFlowPaymentPendingInfo {
		[JsonPropertyName("media")]
		public string? Media { get; set; }

		[JsonPropertyName("date")]
		public string? Date { get; set; }
	}

    [ExcludeFromCodeCoverage]
    public class SalFlowPaymentData {
		[JsonPropertyName("date")]
		public string? Date { get; set; }

		[JsonPropertyName("media")]
		public string? Media { get; set; }

		[JsonPropertyName("conversionDate")]
		public string? ConversionDate { get; set; }

		[JsonPropertyName("conversionRate")]
		public string? ConversionRate { get; set; }

		[JsonPropertyName("amount")]
		public string? Amount { get; set; }

		[JsonPropertyName("currency")]
		public string? Currency { get; set; }

		[JsonPropertyName("fee")]
		public string? Fee { get; set; }

		[JsonPropertyName("balance")]
		public decimal? Balance { get; set; }

		[JsonPropertyName("transferDate")]
		public string? TransferDate { get; set; }

		[JsonPropertyName("taxes")]
		public decimal? Taxes { get; set; }
	}
}
