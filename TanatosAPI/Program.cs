using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.SecretsManager;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using TanatosAPI.Endpoints;
using TanatosAPI.Entities.Contexts;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;
using static System.Runtime.InteropServices.JavaScript.JSType;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.Configure<RouteOptions>(options => {
    options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
});

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi, new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>());

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

if (builder.Environment.IsProduction()) {
	string cognitoRegion = Environment.GetEnvironmentVariable("COGNITO_REGION") ?? throw new Exception($"No se ha configurado la variable de entorno COGNITO_REGION.");
	string cognitoBaseUrl = Environment.GetEnvironmentVariable("COGNITO_BASE_URL") ?? throw new Exception($"No se ha configurado la variable de entorno COGNITO_BASE_URL.");
	string cognitoUserPoolId = Environment.GetEnvironmentVariable("COGNITO_USER_POOL_ID") ?? throw new Exception($"No se ha configurado la variable de entorno COGNITO_USER_POOL_ID.");

	builder.Services
        .AddAuthentication("Bearer")
		.AddJwtBearer("Bearer", options => {
			options.Authority = cognitoBaseUrl;
			options.MetadataAddress = $"https://cognito-idp.{cognitoRegion}.amazonaws.com/{cognitoUserPoolId}/.well-known/openid-configuration";
			options.SaveToken = true;
			options.TokenValidationParameters = new TokenValidationParameters {
				ValidateIssuer = true,
				ValidIssuer = cognitoBaseUrl,
				ValidateAudience = false,
				RoleClaimType = "cognito:groups",
			};
		});
} else {
	builder.Services
		.AddAuthentication("AllowAnonymous")
		.AddScheme<AuthenticationSchemeOptions, AllowAnonymousAuthenticationHandler>("AllowAnonymous", options => { });
}

builder.Services.AddAuthorizationBuilder()
	.AddPolicy("Admin", policy => policy.RequireRole("Admin"));

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapTipoReceptorNotificacionEndpoints();

app.Run();

