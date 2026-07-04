using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others.Hermes {
    [ExcludeFromCodeCoverage]
    public class SalHermesWhatsappConversacion {
		[JsonPropertyName("tenantId")]
		public required string TenantId { get; set; }

		[JsonPropertyName("numeroTelefono")]
		public required string NumeroTelefono { get; set; }

		[JsonPropertyName("fechaUltimoMensaje")]
		public DateTime FechaUltimoMensaje { get; set; }

		[JsonPropertyName("previewUltimoMensaje")]
		public string? PreviewUltimoMensaje { get; set; }

		[JsonPropertyName("cantidadNoLeidos")]
		public int CantidadNoLeidos { get; set; }

		[JsonPropertyName("estado")]
		public required string Estado { get; set; }

		[JsonPropertyName("fechaUltimaEntrada")]
		public DateTime? FechaUltimaEntrada { get; set; }

		[JsonPropertyName("puedeResponderGratuitoHasta")]
		public DateTime? PuedeResponderGratuitoHasta { get; set; }
	}
}
