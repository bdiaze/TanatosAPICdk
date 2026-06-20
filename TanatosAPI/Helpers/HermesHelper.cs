using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using TanatosAPI.Entities.Others;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
    public class HermesHelper(IHermesHttpClient httpClient) {

		public async Task<SalHermesEnviar> EnviarCorreo(EntHermesCorreoEnviar correo) {
			using HttpResponseMessage response = await httpClient.PostAsync("Correo/Enviar", new StringContent(JsonSerializer.Serialize(correo, AppJsonSerializerContext.Default.EntHermesCorreoEnviar), Encoding.UTF8, "application/json"));
			if (response.StatusCode != HttpStatusCode.OK) {
				throw new HttpRequestException(
					$"Ocurrió un error al enviar el correo. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}",
					inner: null,
					statusCode: response.StatusCode
				);
			}

			string content = await response.Content.ReadAsStringAsync();

			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalHermesEnviar)!;

		}

		public async Task<SalHermesEnviar> EnviarWhatsapp(EntHermesWhatsappEnviar whatsapp) {
			using HttpResponseMessage response = await httpClient.PostAsync("Whatsapp/Enviar", new StringContent(JsonSerializer.Serialize(whatsapp, AppJsonSerializerContext.Default.EntHermesWhatsappEnviar), Encoding.UTF8, "application/json"));
			if (response.StatusCode != HttpStatusCode.OK) {
				throw new HttpRequestException(
					$"Ocurrió un error al enviar el whatsapp. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}",
					inner: null,
					statusCode: response.StatusCode
				);
			}

			string content = await response.Content.ReadAsStringAsync();

			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalHermesEnviar)!;
		}

		public async Task<SalHermesWhatsappMedia> ObtenerMedia(string whatsappMessageId) {
			using HttpResponseMessage response = await httpClient.GetAsync($"Whatsapp/Media/{Uri.EscapeDataString(whatsappMessageId)}");
			if (response.StatusCode != HttpStatusCode.OK) {
				throw new HttpRequestException(
					$"Ocurrió un error al obtener media de whatsapp. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}",
					inner: null,
					statusCode: response.StatusCode
				);
			}

			string content = await response.Content.ReadAsStringAsync();

			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalHermesWhatsappMedia)!;
		}

		public async Task<List<SalHermesWhatsappConversacion>> ObtenerConversaciones(string tenantId, DateTime? desde, DateTime? hasta) {
			string url = $"Whatsapp/Conversaciones/{Uri.EscapeDataString(tenantId)}";
			if (desde != null) url += $"/{Uri.EscapeDataString(desde.Value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture))}";
			if (hasta != null) url += $"/{Uri.EscapeDataString(hasta.Value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture))}";

			using HttpResponseMessage response = await httpClient.GetAsync(url);
			if (response.StatusCode != HttpStatusCode.OK) {
				throw new HttpRequestException(
					$"Ocurrió un error al obtener conversaciones de whatsapp. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}",
					inner: null,
					statusCode: response.StatusCode
				);
			}

			string content = await response.Content.ReadAsStringAsync();

			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.ListSalHermesWhatsappConversacion)!;
		}

		public async Task<List<SalHermesWhatsappMensaje>> ObtenerMensajes(string tenantId, string numeroTelefono, DateTime? desde, DateTime? hasta) {
			string url = $"Whatsapp/Mensajes/{Uri.EscapeDataString(tenantId)}/{Uri.EscapeDataString(numeroTelefono)}";
			if (desde != null) url += $"/{Uri.EscapeDataString(desde.Value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture))}";
			if (hasta != null) url += $"/{Uri.EscapeDataString(hasta.Value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture))}";

			using HttpResponseMessage response = await httpClient.GetAsync(url);
			if (response.StatusCode != HttpStatusCode.OK) {
				throw new HttpRequestException(
					$"Ocurrió un error al obtener mensajes de conversación de whatsapp. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}",
					inner: null,
					statusCode: response.StatusCode
				);
			}

			string content = await response.Content.ReadAsStringAsync();

			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.ListSalHermesWhatsappMensaje)!;
		}
	}
}
