namespace TanatosAPI.Entities.Others {
	public class EntHermesWhatsappEnviar {
		public required string De { get; set; }
		public required string Para { get; set; }
		public required string NombreTemplate { get; set; }
		public string? Lenguaje { get; set; }
		public string[]? ParametrosTitulo {  get; set; }
		public string[]? ParametrosCuerpo { get; set; }
		public string[]? ParametrosBoton { get; set; }
	}
}
