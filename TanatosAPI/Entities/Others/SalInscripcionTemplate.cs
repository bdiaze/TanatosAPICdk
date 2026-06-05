using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class SalInscripcionTemplate {
		public required long IdTemplate { get; set; }
		public string? NombreTemplate { get; set; }

	}
}
