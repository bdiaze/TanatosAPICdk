using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.SecretsManager;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TanatosAPI.Entities.CompiledModels;
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
    if (!env.IsDevelopment()) {
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

    if (!env.IsDevelopment()) {
        connectionString += "Ssl Mode=Require;";
        connectionString += "Trust Server Certificate=true;";
    }

    options.UseNpgsql(connectionString);
    options.UseModel(TanatosDbContextModel.Instance);
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.Run();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

