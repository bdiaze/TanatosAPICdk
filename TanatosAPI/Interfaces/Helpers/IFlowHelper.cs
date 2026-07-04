using TanatosAPI.Entities.Others.Flow;

namespace TanatosAPI.Interfaces.Helpers {
	public interface IFlowHelper {
		public Task<SalFlowPlanCreate> PlanCreate(string planId, string nombre, decimal monto, int cantMeses, short diasAntesVencer = 3, short reintentos = 3);
		public Task<SalFlowPlanEdit> PlanEdit(string planId, string nombre, decimal monto, int cantMeses, short diasAntesVencer = 3, short reintentos = 3);
		public Task<SalFlowPlanDelete> PlanDelete(string planId);
		public Task<SalFlowCustomerCreate> CustomerCreate(string nombre, string correo, string sub);
		public Task<SalFlowUrlToken> CustomerRegister(string customerId);
		public Task<SalFlowCustomerGetRegisterStatus> CustomerGetRegisterStatus(string token);
		public Task<SalFlowSubscriptionCreate> SubscriptionCreate(string planId, string customerId, DateTime? fechaInicioUtc = null);
		public Task<SalFlowSubscriptionGet> SubscriptionGet(string subscriptionId);
		public Task<SalFlowSubscriptionCancel> SubscriptionCancel(string subscriptionId, short atPeriodEnd = 1);
		public Task<SalFlowPaymentGetStatus> PaymentGetStatus(string token);
		public Task<SalFlowInvoiceGet> InvoiceGet(string invoiceId);
	}
}
