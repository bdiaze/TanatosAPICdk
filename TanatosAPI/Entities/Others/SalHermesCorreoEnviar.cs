using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others {
	public class SalHermesCorreoEnviar {
		[JsonPropertyName("idMensaje")]
		public required string IdMensaje { get; set; }
	}
}
