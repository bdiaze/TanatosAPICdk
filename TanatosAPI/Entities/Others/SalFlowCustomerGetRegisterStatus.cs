using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others {
	public class SalFlowCustomerGetRegisterStatus {
		[JsonPropertyName("status")]
		/* 0: No registrado - 1: Registrado */
		public string? Status { get; set; }

		[JsonPropertyName("customerId")]
		public string? CustomerId { get; set; }

		[JsonPropertyName("creditCardType")]
		public string? CreditCardType { get; set; }

		[JsonPropertyName("last4CardDigits")]
		public string? Last4CardDigits { get; set; }
	}
}
