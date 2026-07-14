using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.Suscripcion {
	[ExcludeFromCodeCoverage]
	public class SalSuscripcionResumen {
		public required bool TienePlanEmpresa { get; set; }
		public string? NombrePlanEnCurso { get; set; }
		public decimal? PrecioPlanEnCurso { get; set; }
		public string? NombrePlanPagoEnCurso { get; set; }
		public decimal? PrecioPlanPagoEnCurso { get; set; }
		public DateTime? FechaExpiracion { get; set; }
		public DateTime? FechaProximoCobro { get; set; }
		public required bool RenovacionAutomatica { get; set; }
	}
}
