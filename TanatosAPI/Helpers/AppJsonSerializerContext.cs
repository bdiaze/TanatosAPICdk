using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Helpers {
	[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
	[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
	[JsonSerializable(typeof(ProblemDetails))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(TipoReceptorNotificacion))]
	[JsonSerializable(typeof(List<TipoReceptorNotificacion>))]
	internal partial class AppJsonSerializerContext : JsonSerializerContext {
    }
}
