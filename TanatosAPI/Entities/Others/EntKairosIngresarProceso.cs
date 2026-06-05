using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class EntKairosIngresarProceso {
		public required string Nombre { get; set; }
		public required string Cron { get; set; }
		public required string ArnRol { get; set; }
		public required string ArnProceso { get; set; }
		public required string Parametros { get; set; }
		public required bool Habilitado { get; set; }
	}
}
