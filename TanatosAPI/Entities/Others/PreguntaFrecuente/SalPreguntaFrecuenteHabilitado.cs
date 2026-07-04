using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.PreguntaFrecuente {
	[ExcludeFromCodeCoverage]
	public class SalPreguntaFrecuenteHabilitado {
		public required int Orden { get; set; }
		public required string Pregunta { get; set; }
		public required string Respuesta { get; set; }
	}
}
