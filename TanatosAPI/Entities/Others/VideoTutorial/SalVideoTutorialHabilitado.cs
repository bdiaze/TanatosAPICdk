using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.VideoTutorial {
	[ExcludeFromCodeCoverage]
	public class SalVideoTutorialHabilitado {
		public required int Orden { get; set; }
		public required string Titulo { get; set; }
		public string? Descripcion { get; set; }
		public required string Url { get; set; }
	}
}
