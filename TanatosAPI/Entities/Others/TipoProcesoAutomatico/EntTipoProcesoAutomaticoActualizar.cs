namespace TanatosAPI.Entities.Others.TipoProcesoAutomatico {
	public class EntTipoProcesoAutomaticoActualizar {
		public required long Id { get; set; }
		public required string Nombre { get; set; }
		public string? Descripcion { get; set; }
		public required bool Habilitado { get; set; }
		public required int Orden { get; set; }
	}
}
