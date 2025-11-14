using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.SecretsManager;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using TanatosAPI.Endpoints;
using TanatosAPI.Entities.Contexts;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.Configure<RouteOptions>(options => {
    options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
});

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi, new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "API Tánatos - Minimal API AoT",
        Version = "v1"
    });
});

#region Singleton AWS Services
builder.Services.AddSingleton<IAmazonSecretsManager>(sp => {
    AmazonSecretsManagerConfig config = new() {
        ConnectTimeout = TimeSpan.FromSeconds(5),
        Timeout = TimeSpan.FromSeconds(25)
    };
    return new AmazonSecretsManagerClient(config);
});
#endregion

#region Singleton Helpers
builder.Services.AddSingleton<VariableEntornoHelper>();
builder.Services.AddSingleton<SecretManagerHelper>();
builder.Services.AddSingleton<ConnectionStringHelper>();
builder.Services.AddSingleton<DatabaseConnectionHelper>();
#endregion

#region Singleton DAO
builder.Services.AddSingleton<TipoReceptorNotificacionDao>();
#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapTipoReceptorNotificacionEndpoints();

app.Run();

