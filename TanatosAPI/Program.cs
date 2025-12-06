using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.SecretsManager;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
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

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

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

	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
		Name = "Authorization",
		Description = "JWT Authorization header usando el esquema Bearer.\r\n\r\n" +
					  "Ejemplo: \"Bearer eyJhbGciOi...\"",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT"
	});

	c.OperationFilter<AuthApplyOperationFilter>();
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
builder.Services.AddSingleton<CategoriaNormaDao>();
builder.Services.AddSingleton<DestinatarioNotificacionDao>();
builder.Services.AddSingleton<InscripcionTemplateDao>();
builder.Services.AddSingleton<TemplateDao>();
builder.Services.AddSingleton<TemplateNormaDao>();
builder.Services.AddSingleton<TemplateNormaFiscalizadorDao>();
builder.Services.AddSingleton<TemplateNormaNotificacionDao>();
builder.Services.AddSingleton<TipoFiscalizadorDao>();
builder.Services.AddSingleton<TipoPeriodicidadDao>();
builder.Services.AddSingleton<TipoReceptorNotificacionDao>();
builder.Services.AddSingleton<TipoUnidadTiempoDao>();
#endregion

string cognitoRegion;
string cognitoBaseUrl;
string cognitoUserPoolId;
if (builder.Environment.IsDevelopment()) {
	cognitoRegion = builder.Configuration[$"VariableEntorno:COGNITO_REGION"] ?? throw new Exception($"Debes agregar el atributo VariableEntorno > COGNITO_REGION en el archivo appsettings.Development.json para ejecutar localmente.");
	cognitoBaseUrl = builder.Configuration[$"VariableEntorno:COGNITO_BASE_URL"] ?? throw new Exception($"Debes agregar el atributo VariableEntorno > COGNITO_BASE_URL en el archivo appsettings.Development.json para ejecutar localmente.");
	cognitoUserPoolId = builder.Configuration[$"VariableEntorno:COGNITO_USER_POOL_ID"] ?? throw new Exception($"Debes agregar el atributo VariableEntorno > COGNITO_USER_POOL_ID en el archivo appsettings.Development.json para ejecutar localmente.");
} else {
	cognitoRegion = Environment.GetEnvironmentVariable("COGNITO_REGION") ?? throw new Exception($"No se ha configurado la variable de entorno COGNITO_REGION.");
	cognitoBaseUrl = Environment.GetEnvironmentVariable("COGNITO_BASE_URL") ?? throw new Exception($"No se ha configurado la variable de entorno COGNITO_BASE_URL.");
	cognitoUserPoolId = Environment.GetEnvironmentVariable("COGNITO_USER_POOL_ID") ?? throw new Exception($"No se ha configurado la variable de entorno COGNITO_USER_POOL_ID.");
}

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
			NameClaimType = ClaimTypes.NameIdentifier,
			RoleClaimType = ClaimTypes.Role,
		};
		options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents {
			OnTokenValidated = context => {
				if (context.Principal!.Identity is not ClaimsIdentity identity) {
					return Task.CompletedTask;
				}

				List<Claim> groupClaims = identity.FindAll("cognito:groups").ToList();
				foreach (Claim claim in groupClaims) {
					identity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
					identity.RemoveClaim(claim);
				}

				return Task.CompletedTask;
			}
		};
	});

builder.Services.AddAuthorizationBuilder()
	.AddPolicy("Admin", policy => policy.RequireRole("Admin"));

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapCategoriaNormaEndpoints();
app.MapTipoFiscalizadorEndpoints();
app.MapTipoPeriodicidadEndpoints();
app.MapTipoReceptorNotificacionEndpoints();
app.MapTipoUnidadTiempoEndpoints();
app.MapAuthEndpoints();

app.Run();

