using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TanatosAPI.Entities.Others;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TanatosAPI.Helpers {
	public class FlowHelper(VariableEntornoHelper variableEntorno, SecretManagerHelper secretManagerHelper, HttpClient httpClient) {
		private readonly string _flowBaseUrl = variableEntorno.Obtener("FLOW_API_URL");
		private readonly string _flowApiKey = JsonSerializer.Deserialize(secretManagerHelper.ObtenerSecreto(variableEntorno.Obtener("SECRET_ARN_APP")).Result, AppJsonSerializerContext.Default.DictionaryStringString)!["FlowApiKey"];
		private readonly string _flowSecretKey = JsonSerializer.Deserialize(secretManagerHelper.ObtenerSecreto(variableEntorno.Obtener("SECRET_ARN_APP")).Result, AppJsonSerializerContext.Default.DictionaryStringString)!["FlowSecretKey"];

		public async Task<SalFlowPlanCreate> PlanCreate(string planId, string nombre, decimal monto, int cantMeses, short diasAntesVencer = 3, short reintentos = 3) {
			Dictionary<string, string> parametros = new() {
				["apiKey"] = _flowApiKey,
				["planId"] = planId,
				["name"] = nombre,
				["amount"] = monto.ToString(CultureInfo.InvariantCulture),
				["interval"] = "3", // Mensual
				["interval_count"] = cantMeses.ToString(CultureInfo.InvariantCulture),
				["days_until_due"] = diasAntesVencer.ToString(CultureInfo.InvariantCulture),
				["urlCallback"] = $"{variableEntorno.Obtener("FLOW_URL_CALLBACK")}/PlanCreate",
				["charges_retries_number"] = reintentos.ToString(CultureInfo.InvariantCulture),
			};
			parametros["s"] = Firmar(parametros);
			FormUrlEncodedContent formContent = new(parametros);

			using HttpResponseMessage response = await httpClient.PostAsync(_flowBaseUrl + $"plans/create", formContent);
			if (!response.IsSuccessStatusCode) {
				throw new Exception($"Ocurrió un error al crear plan en Flow. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
			}

			string content = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalFlowPlanCreate)!;
		}

		public async Task<SalFlowPlanEdit> PlanEdit(string planId, string nombre, decimal monto, int cantMeses, short diasAntesVencer = 3, short reintentos = 3) {
			Dictionary<string, string> parametros = new() {
				["apiKey"] = _flowApiKey,
				["planId"] = planId,
				["name"] = nombre,
				["amount"] = monto.ToString(CultureInfo.InvariantCulture),
				["interval"] = "3", // Mensual
				["interval_count"] = cantMeses.ToString(CultureInfo.InvariantCulture),
				["days_until_due"] = diasAntesVencer.ToString(CultureInfo.InvariantCulture),
				["urlCallback"] = $"{variableEntorno.Obtener("FLOW_URL_CALLBACK")}/PlanEdit",
				["charges_retries_number"] = reintentos.ToString(CultureInfo.InvariantCulture),
			};
			parametros["s"] = Firmar(parametros);
			FormUrlEncodedContent formContent = new(parametros);

			using HttpResponseMessage response = await httpClient.PostAsync(_flowBaseUrl + $"plans/edit", formContent);
			if (!response.IsSuccessStatusCode) {
				throw new Exception($"Ocurrió un error al editar plan en Flow. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
			}

			string content = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalFlowPlanEdit)!;
		}

		public async Task<SalFlowPlanDelete> PlanDelete(string planId) {
			Dictionary<string, string> parametros = new() {
				["apiKey"] = _flowApiKey,
				["planId"] = planId,
			};
			parametros["s"] = Firmar(parametros);
			FormUrlEncodedContent formContent = new(parametros);

			using HttpResponseMessage response = await httpClient.PostAsync(_flowBaseUrl + $"plans/delete", formContent);
			if (!response.IsSuccessStatusCode) {
				throw new Exception($"Ocurrió un error al eliminar plan en Flow. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
			}

			string content = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalFlowPlanDelete)!;
		}

		public async Task<SalFlowCustomerCreate> CustomerCreate(string nombre, string correo, string sub) {
			Dictionary<string, string> parametros = new() {
				["apiKey"] = _flowApiKey,
				["name"] = nombre,
				["email"] = correo,
				["externalId"] = sub,
			};
			parametros["s"] = Firmar(parametros);
			FormUrlEncodedContent formContent = new(parametros);

			using HttpResponseMessage response = await httpClient.PostAsync(_flowBaseUrl + $"customer/create", formContent);
			if (!response.IsSuccessStatusCode) {
				throw new Exception($"Ocurrió un error al crear usuario en Flow. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
			}

			string content = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalFlowCustomerCreate)!;
		}

		public async Task<SalFlowUrlToken> CustomerRegister(string customerId) {
			Dictionary<string, string> parametros = new() {
				["apiKey"] = _flowApiKey,
				["customerId"] = customerId,
				["url_return"] = $"{variableEntorno.Obtener("FLOW_URL_CALLBACK")}/CustomerRegister",
			};
			parametros["s"] = Firmar(parametros);
			FormUrlEncodedContent formContent = new(parametros);

			using HttpResponseMessage response = await httpClient.PostAsync(_flowBaseUrl + $"customer/register", formContent);
			if (!response.IsSuccessStatusCode) {
				throw new Exception($"Ocurrió un error al registrar tarjeta de usuario en Flow. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
			}

			string content = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalFlowUrlToken)!;
		}

		public async Task<SalFlowCustomerGetRegisterStatus> CustomerGetRegisterStatus(string token) {
			Dictionary<string, string> parametros = new() {
				["apiKey"] = _flowApiKey,
				["token"] = token,
			};
			parametros["s"] = Firmar(parametros);
			string query = await new FormUrlEncodedContent(parametros).ReadAsStringAsync();

			using HttpResponseMessage response = await httpClient.GetAsync(_flowBaseUrl + $"customer/getRegisterStatus?{query}");
			if (!response.IsSuccessStatusCode) {
				throw new Exception($"Ocurrió un error al obtener estado de registro de tarjeta de usuario en Flow. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
			}

			string content = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalFlowCustomerGetRegisterStatus)!;
		}

		public async Task<SalFlowSubscriptionCreate> SuscriptionCreate(string planId, string customerEmail) {
			Dictionary<string, string> parametros = new() {
				["apiKey"] = _flowApiKey,
				["planId"] = planId,
				["customerEmail"] = customerEmail,
			};
			parametros["s"] = Firmar(parametros);
			FormUrlEncodedContent formContent = new(parametros);

			using HttpResponseMessage response = await httpClient.PostAsync(_flowBaseUrl + $"subscription/create", formContent);
			if (!response.IsSuccessStatusCode) {
				throw new Exception($"Ocurrió un error al crear suscripción en Flow. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
			}

			string content = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalFlowSubscriptionCreate)!;
		}

		public async Task<SalFlowSubscriptionGet> SubscriptionGet(string subscriptionId) {
			Dictionary<string, string> parametros = new() {
				["apiKey"] = _flowApiKey,
				["subscriptionId"] = subscriptionId,
			};
			parametros["s"] = Firmar(parametros);
			string query = await new FormUrlEncodedContent(parametros).ReadAsStringAsync();

			using HttpResponseMessage response = await httpClient.GetAsync(_flowBaseUrl + $"subscription/get?{query}");
			if (!response.IsSuccessStatusCode) {
				throw new Exception($"Ocurrió un error al obtener suscripción en Flow. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
			}

			string content = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalFlowSubscriptionGet)!;
		}

		public async Task<SalFlowSubscriptionCancel> SuscriptionCancel(string subscriptionId, short atPeriodEnd = 1) {
			Dictionary<string, string> parametros = new() {
				["apiKey"] = _flowApiKey,
				["subscriptionId"] = subscriptionId,
				["at_period_end"] = atPeriodEnd.ToString(CultureInfo.InvariantCulture),
			};
			parametros["s"] = Firmar(parametros);
			FormUrlEncodedContent formContent = new(parametros);

			using HttpResponseMessage response = await httpClient.PostAsync(_flowBaseUrl + $"subscription/cancel", formContent);
			if (!response.IsSuccessStatusCode) {
				throw new Exception($"Ocurrió un error al cancelar suscripción en Flow. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
			}

			string content = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalFlowSubscriptionCancel)!;
		}

		public async Task<SalFlowPaymentGetStatus> PaymentGetStatus(string token) {
			Dictionary<string, string> parametros = new() {
				["apiKey"] = _flowApiKey,
				["token"] = token,
			};
			parametros["s"] = Firmar(parametros);
			string query = await new FormUrlEncodedContent(parametros).ReadAsStringAsync();

			using HttpResponseMessage response = await httpClient.GetAsync(_flowBaseUrl + $"payment/getStatus?{query}");
			if (!response.IsSuccessStatusCode) {
				throw new Exception($"Ocurrió un error al obtener estado de pago en Flow. StatusCode: {response.StatusCode} - Content: {await response.Content.ReadAsStringAsync()}");
			}

			string content = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize(content, AppJsonSerializerContext.Default.SalFlowPaymentGetStatus)!;
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
