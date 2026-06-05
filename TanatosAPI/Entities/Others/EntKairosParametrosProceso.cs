using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class EntKairosParametrosProceso {
		public required long IdNormaSuscrita { get; set; }
		public required string Cron { get; set; }
		public long? IdTipoUnidadTiempoAntelacion { get; set; }
        public int? CantAntelacion { get; set; }
		public bool? EsVencimiento { get; set; }
		public required bool ProgramarSiguienteEjecucion { get; set; }
	}
}
