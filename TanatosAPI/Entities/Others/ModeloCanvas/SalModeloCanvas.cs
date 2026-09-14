namespace TanatosAPI.Entities.Others.ModeloCanvas {
	public class SalModeloCanvas {
		public required long Id { get; set; }
		public required string? SociosClave { get; set; }
		public required string? ActividadesClave { get; set; }
		public required string? RecursosClave { get; set; }
		public required string? PropuestaValor { get; set; }
		public required string? RelacionesClientes { get; set; }
		public required string? Canales { get; set; }
		public required string? SegmentosClientes { get; set; }
		public required string? EstructuraCostos { get; set; }
		public required string? FuentesIngresos { get; set; }
		public required DateTime FechaCreacion { get; set; }
	}
}
