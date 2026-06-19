namespace TanatosAPI.Entities.Others {
	public class EntPreguntaFrecuenteCrear {
		public required string Pregunta { get; set; }
		public required string Respuesta { get; set; }
		public required bool Habilitado { get; set; }
		public required int Orden { get; set; }
	}
}
