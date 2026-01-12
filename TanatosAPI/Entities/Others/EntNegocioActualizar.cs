namespace TanatosAPI.Entities.Others {
	public class EntNegocioActualizar {
		public required long Id { get; set; }
		public required string Nombre { get; set; }
		public string? Direccion { get; set; }
		public long? IdTipoActividad { get; set; }
	}
}
