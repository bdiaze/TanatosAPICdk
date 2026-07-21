using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others.Kairos {
    [ExcludeFromCodeCoverage]
    public class EntKairosParametrosProceso {
		public required long IdNormaSuscrita { get; set; }
		public string? Cron { get; set; }
        public int? FrecuenciaDias { get; set; }
        public DateTime? InicioEjecucionUtc { get; set; }
        public long? IdTipoUnidadTiempoAntelacion { get; set; }
        public int? CantAntelacion { get; set; }
		public bool? EsVencimiento { get; set; }
		public required bool ProgramarSiguienteEjecucion { get; set; }
	}
}
