using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class EntHermesWhatsappEnviar {
		public required string De { get; set; }
		public required string Para { get; set; }
		public string? NombreTemplate { get; set; }
		public string Lenguaje { get; set; } = "es_CL";
		public string[]? ParametrosTitulo {  get; set; }
		public string[]? ParametrosCuerpo { get; set; }
		public string[]? ParametrosBoton { get; set; }
		public string? Cuerpo { get; set; }
	}
}
