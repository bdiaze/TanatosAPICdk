using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class EntInscripcionTemplateDesactivar {
		public required long IdNegocio { get; set; }

		public required long IdTemplate { get; set; }
	}
}
