namespace TanatosAPI.Entities.Others {
	public class EntKairosParametrosProceso {
		public required long IdNormaSuscrita { get; set; }
		public required string Cron { get; set; }
		public required bool ProgramarSiguienteEjecucion { get; set; }
	}
}
