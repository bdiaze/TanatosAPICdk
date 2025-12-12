using Microsoft.OpenApi.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using TanatosAPI.Entities.Others;

namespace TanatosAPI.Helpers {
	public class HermesHelper(VariableEntornoHelper variableEntorno, ApiKeyHelper apiKey) {
		private readonly string _hermesBaseUrl = variableEntorno.Obtener("HERMES_API_URL");
		private readonly string _hermesApiKey = apiKey.ObtenerApiKey(variableEntorno.Obtener("HERMES_API_KEY_ID")).Result;

		public async Task<SalHermesCorreoEnviar> EnviarCorreo(EntHermesCorreoEnviar correo) {
			using HttpClient client = new();
			client.DefaultRequestHeaders.Add("x-api-key", _hermesApiKey);

			HttpResponseMessage response = await client.PostAsync(_hermesBaseUrl + "Correo/Enviar", new StringContent(JsonSerializer.Serialize(correo, AppJsonSerializerContext.Default.EntHermesCorreoEnviar), Encoding.UTF8, "application/json"));
			if (response.StatusCode != HttpStatusCode.OK) {
				throw new Exception($"Ocurrió un error al enviar el correo. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
			}

			string content = await response.Content.ReadAsStringAsync();

			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalHermesCorreoEnviar)!;

		}
	}
}
