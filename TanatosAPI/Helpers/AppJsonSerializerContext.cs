using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;

namespace TanatosAPI.Helpers {
	[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
	[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
	[JsonSerializable(typeof(ProblemDetails))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
	[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
	[JsonSerializable(typeof(TipoReceptorNotificacion))]
	[JsonSerializable(typeof(List<TipoReceptorNotificacion>))]
	[JsonSerializable(typeof(EntAuthObtenerAccessToken))]
	[JsonSerializable(typeof(SalAuthObtenerAccessToken))]
	[JsonSerializable(typeof(SalAuthRefreshAccessToken))]
	internal partial class AppJsonSerializerContext : JsonSerializerContext {
    }
}
