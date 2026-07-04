using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.Suscripcion {
    [ExcludeFromCodeCoverage]
    public class SalSuscripcion {
		public required long Id { get; set; }
		public required long IdPlan { get; set; }
		public required string NombrePlan { get; set; }
		public required decimal PrecioPlan { get; set; }
		public required int DuracionMesesPlan { get; set; }
		public DateTime? FechaInicio { get; set; }
		public DateTime? FechaExpiracion { get; set; }
		public DateTime? FechaCancelacion { get; set; }
		public required short Estado { get; set; }
		public required bool TieneFlowSubscriptionId { get; set; }
	}
}
