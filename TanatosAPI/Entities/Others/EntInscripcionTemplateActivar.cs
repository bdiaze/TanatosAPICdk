using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class EntInscripcionTemplateActivar {
		public required long IdNegocio { get; set; }

		public required long IdTemplate { get; set; }

		public required bool ActivarPadres { get; set; } = false;
	}
}
