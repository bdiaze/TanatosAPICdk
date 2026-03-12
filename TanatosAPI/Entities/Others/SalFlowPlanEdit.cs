using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others {
	public class SalFlowPlanEdit {
		[JsonPropertyName("planId")]
		public string? PlanId { get; set; }

		[JsonPropertyName("name")]
		public string? Name { get; set; }

		[JsonPropertyName("currency")]
		public string? Currency { get; set; }

		[JsonPropertyName("amount")]
		public string? Amount { get; set; }

		/* 1: Diaria - 2: Semanal - 3: Mensual - 4: Anual */
		[JsonPropertyName("interval")]
		public short? Interval { get; set; }

		[JsonPropertyName("interval_count")]
		public int? IntervalCount { get; set; }

		[JsonPropertyName("created")]
		public string? Created { get; set; }

		[JsonPropertyName("trial_period_days")]
		public short? TrialPeriodDays { get; set; }

		[JsonPropertyName("days_until_due")]
		public short? DaysUntilDue { get; set; }

		[JsonPropertyName("periods_number")]
		public short? PeriodsNumber { get; set; }

		[JsonPropertyName("urlCallback")]
		public string? UrlCallback { get; set; }

		[JsonPropertyName("charges_retries_number")]
		public short? ChargesRetriesNumber { get; set; }

		[JsonPropertyName("currency_convert_option")]
		public short? CurrencyConvertOption { get; set; }

		[JsonPropertyName("status")]
		public short? Status { get; set; }

		[JsonPropertyName("public")]
		public short? Public { get; set; }
	}
}
