using System.Diagnostics;
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
			response.EnsureSuccessStatusCode();
			return JsonSerializer.Deserialize(await response.Content.ReadAsStringAsync(), AppJsonSerializerContext.Default.SalKairosIngresarProceso)!;
		}

		public async Task<List<SalKairosIngresarProceso>> IngresarVariosProcesos(List<EntKairosIngresarProceso> procesos) {
			if (procesos.Count == 0) return []; 

			HttpResponseMessage response = await httpClient.PostAsync("Procesos/Varios", new StringContent(JsonSerializer.Serialize(procesos, AppJsonSerializerContext.Default.ListEntKairosIngresarProceso), Encoding.UTF8, "application/json"));
			response.EnsureSuccessStatusCode();
			return JsonSerializer.Deserialize(await response.Content.ReadAsStringAsync(), AppJsonSerializerContext.Default.ListSalKairosIngresarProceso)!;
		}

		public async Task EliminarProceso(string idProceso) {
			HttpResponseMessage response = await httpClient.DeleteAsync($"Procesos/{Uri.EscapeDataString(idProceso)}");
			response.EnsureSuccessStatusCode();
		}

		public async Task EliminarVariosProcesos(List<string> idsProcesos) {
			if (idsProcesos.Count == 0) return;

			HttpRequestMessage request = new(HttpMethod.Delete, "Procesos/Varios") { 
				Content = JsonContent.Create(idsProcesos, AppJsonSerializerContext.Default.ListString)
			};

			HttpResponseMessage response = await httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}
	}
}
