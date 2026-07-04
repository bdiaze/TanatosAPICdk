using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.VideoTutorial {
	[ExcludeFromCodeCoverage]
	public class EntVideoTutorialActualizar {
		public required long Id { get; set; }
		public required string Titulo { get; set; }
		public string? Descripcion { get; set; }
		public required string Url { get; set; }
		public required bool Habilitado { get; set; }
		public required int Orden { get; set; }
	}
}
