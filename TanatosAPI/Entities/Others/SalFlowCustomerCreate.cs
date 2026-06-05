using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class SalFlowCustomerCreate {
		[JsonPropertyName("customerId")]
		public string? CustomerId { get; set; }

		[JsonPropertyName("created")]
		public string? Created { get; set; }

		[JsonPropertyName("email")]
		public string? Email { get; set; }

		[JsonPropertyName("name")]
		public string? Name { get; set; }

		[JsonPropertyName("pay_mode")]
		public string? PayMode { get; set; }

		[JsonPropertyName("creditCardType")]
		public string? CreditCardType { get; set; }

		[JsonPropertyName("last4CardDigits")]
		public string? Last4CardDigits { get; set; }

		[JsonPropertyName("externalId")]
		public string? ExternalId { get; set; }

		[JsonPropertyName("status")]
		/* 0: Eliminado - 1: Activo */
		public short? Status { get; set; }

		[JsonPropertyName("registerDate")]
		public string? RegisterDate { get; set; }
	}
}
