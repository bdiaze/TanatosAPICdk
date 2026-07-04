using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.NormaSuscrita {
    [ExcludeFromCodeCoverage]
    public class EntNormaSuscritaCompletarNorma {
		public required long IdNormaSuscrita { get; set; }
		public required long IdHistorialNormaSuscrita { get; set; }
	}
}
