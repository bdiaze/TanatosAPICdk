namespace TanatosAPI.Entities.Others.Mensaje {
	public class SalMensaje {
		public string? Sub { get; set; }
		public required string Nombre { get; set; }
		public required string Correo { get; set; }
		public required string Contenido { get; set; }
		public required DateTime FechaCreacion { get; set; }
	}
}
