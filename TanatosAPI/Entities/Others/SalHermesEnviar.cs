using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class SalHermesEnviar {
		[JsonPropertyName("idMensaje")]
		public required string IdMensaje { get; set; }
	}
}
