using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others.Kairos {
    [ExcludeFromCodeCoverage]
    public class SalKairosIngresarProceso {
		public required string IdProceso { get; set; }
		public required string IdCalendarizacion { get; set; }
		public required string Nombre { get; set; }
		public required string ArnRol { get; set; }
		public required string ArnProceso { get; set; }
		public required string Parametros { get; set; }
		public required string? Cron { get; set; }
		public required int? FrecuenciaDias { get; set; }
		public required DateTime? InicioEjecucionUtc { get; set; }
	}
}
