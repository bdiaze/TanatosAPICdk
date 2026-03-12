using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others {
	public class SalFlowSubscriptionGet {
		[JsonPropertyName("subscriptionId")]
		public string? SubscriptionId { get; set; }

		[JsonPropertyName("planId")]
		public string? PlanId { get; set; }

		[JsonPropertyName("plan_name")]
		public string? PlanName { get; set; }

		[JsonPropertyName("customerId")]
		public string? CustomerId { get; set; }

		[JsonPropertyName("created")]
		public string? Created { get; set; }

		[JsonPropertyName("subscription_start")]
		public string? SubscriptionStart { get; set; }

		[JsonPropertyName("subscription_end")]
		public string? SubscriptionEnd { get; set; }

		[JsonPropertyName("period_start")]
		public string? PeriodStart { get; set; }

		[JsonPropertyName("period_end")]
		public string? PeriodEnd { get; set; }

		[JsonPropertyName("next_invoice_date")]
		public string? NextInvoiceDate { get; set; }

		[JsonPropertyName("trial_period_days")]
		public short? TrialPeriodDays { get; set; }

		[JsonPropertyName("trial_start")]
		public string? TrialStart { get; set; }

		[JsonPropertyName("trial_end")]
		public string? TrialEnd { get; set; }

		[JsonPropertyName("cancel_at_period_end")]
		public short? CancelAtPeriodEnd { get; set; }

		[JsonPropertyName("cancel_at")]
		public string? CancelAt { get; set; }

		[JsonPropertyName("periods_number")]
		public short? PeriodsNumber { get; set; }

		[JsonPropertyName("days_until_due")]
		public short? DaysUntilDue { get; set; }

		[JsonPropertyName("status")]
		/* 0: Inactivo - 1: Activa - 2: En periodo de trial - 4: Cancelada */
		public string? Status { get; set; }

		[JsonPropertyName("discount_balance")]
		public string? DiscountBalance { get; set; }

		[JsonPropertyName("newPlanId")]
		public long? NewPlanId { get; set; }

		[JsonPropertyName("new_plan_scheduled_change_date")]
		public string? NewPlanScheduledChangeDate { get; set; }

		[JsonPropertyName("in_new_plan_next_attempt_date")]
		public string? InNewPlanNextAttemptDate { get; set; }

		[JsonPropertyName("morose")]
		/* 0: Todos los invoices están pagados - 1: Si uno o más invoices están vencidos - 2: Si uno o más invoices están pendientes de pagos pero no vencidos */
		public short? Morose { get; set; }
	}
}
