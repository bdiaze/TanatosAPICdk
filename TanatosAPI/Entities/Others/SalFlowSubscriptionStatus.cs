namespace TanatosAPI.Entities.Others {
	public class SalFlowSubscriptionStatus {
		public required long SubscriptionId { get; set; }
		public required string PlanId { get; set; }
		public required string CustomerId { get; set; }
		public required string ExternalId { get; set; }
		public required int Status { get; set; }
		public required int Period { get; set; }
		public required decimal Amount { get; set; }
		public required string Currency { get; set; }
		public required DateTime Created { get; set; }
		public DateTime? NextPayment { get; set; }
		public DateTime? LastPayment { get; set; }
	}
}
