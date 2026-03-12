namespace TanatosAPI.Entities.Others {
	public class SalPlan {
		public required long Id { get; set; }
		public required string Nombre { get; set; }
		public required decimal Precio { get; set; }
		public required int DuracionMeses { get; set; }
		public required bool SuscripcionUnica { get; set; }
	}
}
