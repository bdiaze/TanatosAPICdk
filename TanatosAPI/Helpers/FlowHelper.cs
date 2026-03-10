using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TanatosAPI.Entities.Others;

namespace TanatosAPI.Helpers {
	public class FlowHelper(VariableEntornoHelper variableEntorno, SecretManagerHelper secretManagerHelper, HttpClient httpClient) {
		private readonly string _flowBaseUrl = variableEntorno.Obtener("FLOW_API_URL");
		private readonly string _flowApiKey = JsonSerializer.Deserialize(secretManagerHelper.ObtenerSecreto("SECRET_ARN_APP").Result, AppJsonSerializerContext.Default.DictionaryStringString)!["FlowApiKey"];
		private readonly string _flowSecretKey = JsonSerializer.Deserialize(secretManagerHelper.ObtenerSecreto("SECRET_ARN_APP").Result, AppJsonSerializerContext.Default.DictionaryStringString)!["FlowSecretKey"];

		public async Task<(string token, string url)> SuscriptionCreate(string customerEmail, string planId, string externalId) {
			Dictionary<string, string> parametros = new() {
				["apiKey"] = _flowApiKey,
				["planId"] = planId,
				["customerEmail"] = customerEmail,
				["externalId"] = externalId,
				["urlConfirmation"] = variableEntorno.Obtener("FLOW_URL_CONFIRMACION"),
				["urlReturn"] = variableEntorno.Obtener("FLOW_URL_RETORNO"),
			};
			parametros["s"] = Firmar(parametros);
			FormUrlEncodedContent formContent = new(parametros);

			using HttpResponseMessage response = await httpClient.PostAsync(_flowBaseUrl + $"subscription/create", formContent);
			if (!response.IsSuccessStatusCode) {
				throw new Exception($"Ocurrió un error al crear suscripción en Flow. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
			}

			string content = await response.Content.ReadAsStringAsync();
			SalFlowSubscriptionCreate salida = JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalFlowSubscriptionCreate)!;
			return (salida.Token, salida.Url);
		}

		private string Firmar(Dictionary<string, string> data) {
			IOrderedEnumerable<KeyValuePair<string, string>> dataOrdenada = data.OrderBy(x => x.Key);
			string aFirmar = string.Join("&", dataOrdenada.Select(x => $"{x.Key}={x.Value}"));

			using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(_flowSecretKey));
			byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(aFirmar));

			return BitConverter.ToString(hash).Replace("-", "").ToLower();
		}
	}
}
