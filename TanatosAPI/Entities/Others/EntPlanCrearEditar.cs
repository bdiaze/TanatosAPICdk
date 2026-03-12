namespace TanatosAPI.Entities.Others {
	public class EntPlanCrearEditar {
		public long Id { get; set; }
		public required string Nombre { get; set; }
		public required decimal Precio { get; set; }
		public required int DuracionMeses { get; set; }
		public required bool Vigencia { get; set; }
	}
}
