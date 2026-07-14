namespace TanatosAPI.Entities.Others.Evaluacion {
	public class SalEvaluacionCrear {
		public required short Puntaje { get; set; }
		public string? Comentario { get; set; }
		public required DateTime FechaCreacion { get; set; }
	}
}
