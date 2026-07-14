namespace TanatosAPI.Entities.Others.Evaluacion {
	public class SalEvaluacion {
		public required string Sub { get; set; }
		public required short Puntaje { get; set; }
		public string? Comentario { get; set; }
		public required DateTime FechaCreacion { get; set; }
	}
}
