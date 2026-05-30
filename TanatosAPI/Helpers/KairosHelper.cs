using System.Net;
using System.Text;
using System.Text.Json;
using TanatosAPI.Entities.Others;

namespace TanatosAPI.Helpers {
	public class KairosHelper(VariableEntornoHelper variableEntorno, ApiKeyHelper apiKey) {
		private readonly string _kairosBaseUrl = variableEntorno.Obtener("KAIROS_API_URL");
		private readonly string _kairosApiKey = apiKey.ObtenerApiKey(variableEntorno.Obtener("KAIROS_API_KEY_ID")).Result;

		public async Task<SalKairosIngresarProceso> IngresarProceso(EntKairosIngresarProceso proceso) {
			using HttpClient client = new();
			client.DefaultRequestHeaders.Add("x-api-key", _kairosApiKey);

			HttpResponseMessage response = await client.PostAsync(_kairosBaseUrl + "Procesos/", new StringContent(JsonSerializer.Serialize(proceso, AppJsonSerializerContext.Default.EntKairosIngresarProceso), Encoding.UTF8, "application/json"));
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
			using HttpClient client = new();
			client.DefaultRequestHeaders.Add("x-api-key", _kairosApiKey);

			HttpResponseMessage response = await client.DeleteAsync(_kairosBaseUrl + $"Procesos/{Uri.EscapeDataString(idProceso)}");
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
