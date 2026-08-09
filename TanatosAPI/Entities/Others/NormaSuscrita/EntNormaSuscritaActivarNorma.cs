using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.NormaSuscrita {
	[ExcludeFromCodeCoverage]
	public class EntNormaSuscritaActivarNorma {
		public required long IdNormaSuscrita { get; set; }
		public required DateTime ProximoVencimiento { get; set; }
	}
}
