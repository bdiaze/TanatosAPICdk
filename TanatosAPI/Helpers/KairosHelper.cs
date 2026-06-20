using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
using TanatosAPI.Entities.Others;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
    public class KairosHelper(IVariableEntornoHelper variableEntorno, IApiKeyHelper apiKey, IKairosHttpClient httpClient) {
		private readonly string _kairosBaseUrl = variableEntorno.Obtener("KAIROS_API_URL");
		private readonly string _kairosApiKey = apiKey.ObtenerApiKey(variableEntorno.Obtener("KAIROS_API_KEY_ID")).Result;

		public async Task<SalKairosIngresarProceso> IngresarProceso(EntKairosIngresarProceso proceso) {
			httpClient.DefaultRequestHeaders.Add("x-api-key", _kairosApiKey);

			HttpResponseMessage response = await httpClient.PostAsync(_kairosBaseUrl + "Procesos/", new StringContent(JsonSerializer.Serialize(proceso, AppJsonSerializerContext.Default.EntKairosIngresarProceso), Encoding.UTF8, "application/json"));
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
			httpClient.DefaultRequestHeaders.Add("x-api-key", _kairosApiKey);

			HttpResponseMessage response = await httpClient.DeleteAsync(_kairosBaseUrl + $"Procesos/{Uri.EscapeDataString(idProceso)}");
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
