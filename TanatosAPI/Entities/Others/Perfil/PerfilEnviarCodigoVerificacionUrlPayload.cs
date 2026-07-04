using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others.Perfil {
	[ExcludeFromCodeCoverage]
	public class PerfilEnviarCodigoVerificacionUrlPayload {
        [JsonPropertyName("correo")]
        public required string Correo { get; set; }

        [JsonPropertyName("codigo")]
        public required string Codigo { get; set; }
    }
}
