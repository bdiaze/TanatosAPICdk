using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others.Hermes {
    [ExcludeFromCodeCoverage]
    public class SalHermesWhatsappMensaje {
		[JsonPropertyName("tenantId")]
		public required string TenantId { get; set; }

		[JsonPropertyName("numeroTelefono")]
		public required string NumeroTelefono { get; set; }

		[JsonPropertyName("idMensaje")]
		public string? IdMensaje { get; set; }

		[JsonPropertyName("whatsappMessageId")]
		public required string WhatsappMessageId { get; set; }

		[JsonPropertyName("direccion")]
		public required string Direccion { get; set; }

		[JsonPropertyName("tipo")]
		public required string Tipo { get; set; }

		[JsonPropertyName("cuerpo")]
		public string? Cuerpo { get; set; }

		[JsonPropertyName("nombreTemplate")]
		public string? NombreTemplate { get; set; }

		[JsonPropertyName("estado")]
		public required string Estado { get; set; }

		[JsonPropertyName("fechaCreacion")]
		public required DateTime FechaCreacion { get; set; }

		[JsonPropertyName("rawPayload")]
		public string? RawPayload { get; set; }
	}
}
