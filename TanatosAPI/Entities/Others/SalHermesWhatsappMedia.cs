using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class SalHermesWhatsappMedia {
		[JsonPropertyName("url")]
		public required string Url { get; set; }
	}
}
