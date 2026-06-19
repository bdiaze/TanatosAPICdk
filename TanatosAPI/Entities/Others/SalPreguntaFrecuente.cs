namespace TanatosAPI.Entities.Others {
	public class SalPreguntaFrecuente {
		public required long Id { get; set; }
		public required string Pregunta { get; set; }
		public required string Respuesta { get; set; }
		public required bool Habilitado { get; set; }
		public required int Orden { get; set; }
	}
}
