namespace TanatosAPI.Entities.Others {
	public class SalEmpleado {
		public required long Id { get; set; }
		public required string Nombre { get; set; }
		public long? IdCargo { get; set; }
		public string? NombreCargo { get; set; }
	}
}
