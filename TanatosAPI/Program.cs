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
#endregion

builder.Services.AddDbContextPool<TanatosDbContext>((serviceProvider, options) => {
    IHostEnvironment env = serviceProvider.GetService<IHostEnvironment>()!;
    IConfiguration config = serviceProvider.GetService<IConfiguration>()!;
    VariableEntornoHelper variableEntorno = serviceProvider.GetRequiredService<VariableEntornoHelper>();
    SecretManagerHelper secretManager = serviceProvider.GetRequiredService<SecretManagerHelper>();

    string appName = variableEntorno.Obtener("APP_NAME");
    Dictionary<string, string> secretConnectionString;
    if (env.IsProduction()) {
        secretConnectionString = JsonSerializer.Deserialize(
            secretManager.ObtenerSecreto(variableEntorno.Obtener("SECRET_ARN_CONNECTION_STRING")).Result,
            AppJsonSerializerContext.Default.DictionaryStringString
        )!;
    } else {
        secretConnectionString = [];
        secretConnectionString.Add("Host", config["ConnectionStrings:Host"]!);
        secretConnectionString.Add("Port", config["ConnectionStrings:Port"]!);
        secretConnectionString.Add($"{appName}Database", config["ConnectionStrings:Database"]!);
        secretConnectionString.Add($"{appName}AppUsername", config["ConnectionStrings:User Id"]!);
        secretConnectionString.Add($"{appName}AppPassword", config["ConnectionStrings:Password"]!);
    }

    string connectionString =
        $"Server={secretConnectionString["Host"]};" +
        $"Port={secretConnectionString["Port"]};" +
        $"Database={secretConnectionString[$"{appName}Database"]};" +
        $"User Id={secretConnectionString[$"{appName}AppUsername"]};" +
        $"Password='{secretConnectionString[$"{appName}AppPassword"]}';";

    if (env.IsProduction()) {
        connectionString += "Ssl Mode=Require;";
        connectionString += "Trust Server Certificate=true;";
    }

    options
        .UseNpgsql(connectionString)
        .UseModel(TanatosAPI.Entities.CompiledModels.TanatosDbContextModel.Instance);
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapTipoReceptorNotificacionEndpoints();

app.Run();

