using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
using TanatosAPI.Entities.Others.Kairos;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Helpers {
    public class KairosHelper(IKairosHttpClient httpClient) : IKairosHelper {
		public async Task<SalKairosIngresarProceso> IngresarProceso(EntKairosIngresarProceso proceso) {
			HttpResponseMessage response = await httpClient.PostAsync("Procesos", new StringContent(JsonSerializer.Serialize(proceso, AppJsonSerializerContext.Default.EntKairosIngresarProceso), Encoding.UTF8, "application/json"));
			if (response.StatusCode != HttpStatusCode.OK) {
				throw new HttpRequestException(
					$"Ocurrió un error al ingresar el proceso. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}",
					inner: null,
					statusCode: response.StatusCode
				);
			}

			return JsonSerializer.Deserialize(await response.Content.ReadAsStringAsync(), AppJsonSerializerContext.Default.SalKairosIngresarProceso)!;
		}

		public async Task EliminarProceso(string idProceso) {
			HttpResponseMessage response = await httpClient.DeleteAsync($"Procesos/{Uri.EscapeDataString(idProceso)}");
			if (response.StatusCode != HttpStatusCode.OK) {
				throw new HttpRequestException(
					$"Ocurrió un error al eliminar el proceso. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}",
					inner: null,
					statusCode: response.StatusCode
				);
			}
		}
	}
}
