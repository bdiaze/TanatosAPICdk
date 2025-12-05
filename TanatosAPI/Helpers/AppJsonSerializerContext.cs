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
	[JsonSerializable(typeof(EntAuthObtenerAccessToken))]
	[JsonSerializable(typeof(SalAuthObtenerAccessToken))]
	[JsonSerializable(typeof(SalAuthRefreshAccessToken))]
	[JsonSerializable(typeof(CategoriaNorma))]
	[JsonSerializable(typeof(List<CategoriaNorma>))]
	[JsonSerializable(typeof(DestinatarioNotificacion))]
	[JsonSerializable(typeof(List<DestinatarioNotificacion>))]
	[JsonSerializable(typeof(InscripcionTemplate))]
	[JsonSerializable(typeof(List<InscripcionTemplate>))]
	[JsonSerializable(typeof(Template))]
	[JsonSerializable(typeof(List<Template>))]
	[JsonSerializable(typeof(TemplateNorma))]
	[JsonSerializable(typeof(List<TemplateNorma>))]
	[JsonSerializable(typeof(TemplateNormaFiscalizador))]
	[JsonSerializable(typeof(List<TemplateNormaFiscalizador>))]
	[JsonSerializable(typeof(TemplateNormaNotificacion))]
	[JsonSerializable(typeof(List<TemplateNormaNotificacion>))]
	[JsonSerializable(typeof(TipoFiscalizador))]
	[JsonSerializable(typeof(List<TipoFiscalizador>))]
	[JsonSerializable(typeof(TipoPeriodicidad))]
	[JsonSerializable(typeof(List<TipoPeriodicidad>))]
	[JsonSerializable(typeof(TipoReceptorNotificacion))]
	[JsonSerializable(typeof(List<TipoReceptorNotificacion>))]
	[JsonSerializable(typeof(TipoUnidadTiempo))]
	[JsonSerializable(typeof(List<TipoUnidadTiempo>))]
	internal partial class AppJsonSerializerContext : JsonSerializerContext {
    }
}
