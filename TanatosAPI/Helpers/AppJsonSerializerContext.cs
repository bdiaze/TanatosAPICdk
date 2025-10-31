using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace TanatosAPI.Helpers {
    [JsonSerializable(typeof(APIGatewayProxyRequest))]
    [JsonSerializable(typeof(APIGatewayProxyResponse))]
    [JsonSerializable(typeof(ProblemDetails))]
    [JsonSerializable(typeof(Todo[]))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext {
    }
}
