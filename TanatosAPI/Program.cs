using Amazon.APIGateway;
using Amazon.CognitoIdentityProvider;
using Amazon.DynamoDBv2;
using Amazon.KeyManagementService;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.SecretsManager;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;
using Scalar.AspNetCore;
using System.Security.Claims;
using TanatosAPI.Business;
using TanatosAPI.Endpoints;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;
using TanatosAPI.UseCases;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
	options.SerializerOptions.MaxDepth = 128;
});

builder.Services.Configure<RouteOptions>(options => {
    options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
});

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi, new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(c => {
	c.AddDocumentTransformer((document, context, cancellationToken) => {
		document.Info = new() {
			Title = "API Tánatos - Minimal API AoT",
			Version = "v1"
		};

		document.Components ??= new();
		document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme> {
			["Bearer"] = new OpenApiSecurityScheme {
				Type = SecuritySchemeType.Http,
				Scheme = "bearer",
				BearerFormat = "JWT",
				In = ParameterLocation.Header,
				Name = "Authorization",
				Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Bearer eyJhbGciOi...\"",
			}
		};

		return Task.CompletedTask;
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
builder.Services.AddSingleton<IAmazonAPIGateway>(sp => {
	AmazonAPIGatewayConfig config = new() {
		ConnectTimeout = TimeSpan.FromSeconds(5),
		Timeout = TimeSpan.FromSeconds(25)
	};
	return new AmazonAPIGatewayClient(config);
});
builder.Services.AddSingleton<IAmazonCognitoIdentityProvider>(sp => {
	AmazonCognitoIdentityProviderConfig config = new() {
		ConnectTimeout = TimeSpan.FromSeconds(5),
		Timeout = TimeSpan.FromSeconds(25)
	};
	return new AmazonCognitoIdentityProviderClient(config);
});
builder.Services.AddSingleton<IAmazonS3>(sp => {
	AmazonS3Config config = new() {
		ConnectTimeout = TimeSpan.FromSeconds(5),
		Timeout = TimeSpan.FromSeconds(25)
	};
	return new AmazonS3Client(config);
});
builder.Services.AddSingleton<IAmazonDynamoDB>(sp => {
    AmazonDynamoDBConfig config = new() {
        ConnectTimeout = TimeSpan.FromSeconds(5),
        Timeout = TimeSpan.FromSeconds(25)
    };
    return new AmazonDynamoDBClient(config);
});
builder.Services.AddSingleton<IAmazonKeyManagementService>(sp => {
	AmazonKeyManagementServiceConfig config = new() {
		ConnectTimeout = TimeSpan.FromSeconds(5),
		Timeout = TimeSpan.FromSeconds(25)
	};
	return new AmazonKeyManagementServiceClient(config);
});
#endregion

#region Singleton Helpers
builder.Services.AddSingleton<IVariableEntornoHelper, VariableEntornoHelper>();
builder.Services.AddSingleton<SecretManagerHelper>();
builder.Services.AddSingleton<IApiKeyHelper, ApiKeyHelper>();
builder.Services.AddHttpClient<ICognitoHttpClient, HttpClientWrapper>();
builder.Services.AddHttpClient<IHermesHttpClient, HttpClientWrapper>();
builder.Services.AddHttpClient<IKairosHttpClient, HttpClientWrapper>();
builder.Services.AddHttpClient<IFlowHttpClient, HttpClientWrapper>();
builder.Services.AddHttpClient<IGoogleRecaptchaHttpClient, HttpClientWrapper>();
builder.Services.AddScoped<ICognitoHelper, CognitoHelper>();
builder.Services.AddScoped<HermesHelper>();
builder.Services.AddScoped<KairosHelper>();
builder.Services.AddScoped<GoogleRecaptchaHelper>();
builder.Services.AddScoped<FlowHelper>();
builder.Services.AddSingleton<ConnectionStringHelper>();
builder.Services.AddSingleton(serviceProvider => {
	ConnectionStringHelper connectionString = serviceProvider.GetRequiredService<ConnectionStringHelper>();
    string connString = connectionString.Obtener().GetAwaiter().GetResult();
    NpgsqlConnectionStringBuilder stringBuilder = new(connString) {
        MaxPoolSize = 5
    };
	return new NpgsqlDataSourceBuilder(stringBuilder.ToString()).Build();
});
builder.Services.AddSingleton<IDatabaseConnectionHelper, DatabaseConnectionHelper>();
builder.Services.AddSingleton<HtmlRenderer>();
builder.Services.AddSingleton<CryptoHelper>();
builder.Services.AddSingleton<IS3Helper, S3Helper>();
builder.Services.AddSingleton<IDocumentoAdjuntoHelper, DocumentoAdjuntoHelper>();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddSingleton<IRateLimiter, DynamoRateLimiter>();
builder.Services.AddSingleton<IKMSHelper, KMSHelper>();
#endregion

#region Singleton DAO
builder.Services.AddScoped<ICategoriaNormaDao, CategoriaNormaDao>();
builder.Services.AddScoped<DestinatarioNotificacionDao>();
builder.Services.AddScoped<InscripcionTemplateDao>();
builder.Services.AddScoped<TemplateDao>();
builder.Services.AddScoped<TemplateNormaDao>();
builder.Services.AddScoped<TemplateNormaFiscalizadorDao>();
builder.Services.AddScoped<TemplateNormaNotificacionDao>();
builder.Services.AddScoped<TemplateActividadDao>();
builder.Services.AddScoped<TipoFiscalizadorDao>();
builder.Services.AddScoped<TipoPeriodicidadDao>();
builder.Services.AddScoped<TipoReceptorNotificacionDao>();
builder.Services.AddScoped<TipoUnidadTiempoDao>();
builder.Services.AddScoped<TipoRubroDao>();
builder.Services.AddScoped<TipoActividadDao>();
builder.Services.AddScoped<NegocioDao>();
builder.Services.AddScoped<NormaSuscritaDao>();
builder.Services.AddScoped<FiscalizadorNormaSuscritaDao>();
builder.Services.AddScoped<NotificacionNormaSuscritaDao>();
builder.Services.AddScoped<HistorialNormaSuscritaDao>();
builder.Services.AddScoped<HistorialNotificacionDao>();
builder.Services.AddScoped<IDocumentoAdjuntoDao, DocumentoAdjuntoDao>();
builder.Services.AddScoped<MensajeDao>();
builder.Services.AddScoped<PlanDao>();
builder.Services.AddScoped<SuscripcionDao>();
builder.Services.AddScoped<EventoPagoDao>();
builder.Services.AddScoped<PagoDao>();
builder.Services.AddScoped<UsuarioDao>();
builder.Services.AddScoped<ICargoDao, CargoDao>();
builder.Services.AddScoped<EmpleadoDao>();
builder.Services.AddScoped<IPreguntaFrecuenteDao, PreguntaFrecuenteDao>();
#endregion

#region Singleton BCP
builder.Services.AddScoped<NormaSuscritaBcp>();
builder.Services.AddScoped<HistorialNormaSuscritaBcp>();
builder.Services.AddScoped<FiscalizadorNormaSuscritaBcp>();
builder.Services.AddScoped<NotificacionNormaSuscritaBcp>();
builder.Services.AddScoped<ProcesoNotificacionBcp>();
builder.Services.AddScoped<DestinatarioNotificacionBcp>();
builder.Services.AddScoped<DocumentoAdjuntoBcp>();
builder.Services.AddScoped<MensajeBcp>();
builder.Services.AddScoped<SuscripcionBcp>();
builder.Services.AddScoped<TemplateNormaBcp>();
builder.Services.AddScoped<INegocioBcp, NegocioBcp>();
builder.Services.AddScoped<UsuarioBcp>();
builder.Services.AddScoped<HistorialNotificacionBcp>();
builder.Services.AddScoped<ICargoBcp, CargoBcp>();
builder.Services.AddScoped<IEmpleadoBcp, EmpleadoBcp>();
builder.Services.AddScoped<ICategoriaNormaBcp, CategoriaNormaBcp>();
builder.Services.AddScoped<IPreguntaFrecuenteBcp, PreguntaFrecuenteBcp>();
#endregion

#region Singleton UseCases
builder.Services.AddScoped<DocumentoAdjuntoUseCase>();
builder.Services.AddScoped<DestinatarioNotificacionUseCase>();
builder.Services.AddScoped<AuthUseCase>();
builder.Services.AddScoped<CargoUseCase>();
builder.Services.AddScoped<CategoriaNormaUseCase>();
builder.Services.AddScoped<PreguntaFrecuenteUseCase>();
#endregion

string cognitoRegion;
string cognitoBaseUrl;
string cognitoUserPoolId;
if (builder.Environment.IsDevelopment()) {
	cognitoRegion = builder.Configuration[$"VariableEntorno:COGNITO_REGION"] ?? throw new InvalidOperationException($"Debes agregar el atributo VariableEntorno > COGNITO_REGION en el archivo appsettings.Development.json para ejecutar localmente.");
	cognitoBaseUrl = builder.Configuration[$"VariableEntorno:COGNITO_BASE_URL"] ?? throw new InvalidOperationException($"Debes agregar el atributo VariableEntorno > COGNITO_BASE_URL en el archivo appsettings.Development.json para ejecutar localmente.");
	cognitoUserPoolId = builder.Configuration[$"VariableEntorno:COGNITO_USER_POOL_ID"] ?? throw new InvalidOperationException($"Debes agregar el atributo VariableEntorno > COGNITO_USER_POOL_ID en el archivo appsettings.Development.json para ejecutar localmente.");
} else {
	cognitoRegion = Environment.GetEnvironmentVariable("COGNITO_REGION") ?? throw new InvalidOperationException($"No se ha configurado la variable de entorno COGNITO_REGION.");
	cognitoBaseUrl = Environment.GetEnvironmentVariable("COGNITO_BASE_URL") ?? throw new InvalidOperationException($"No se ha configurado la variable de entorno COGNITO_BASE_URL.");
	cognitoUserPoolId = Environment.GetEnvironmentVariable("COGNITO_USER_POOL_ID") ?? throw new InvalidOperationException($"No se ha configurado la variable de entorno COGNITO_USER_POOL_ID.");
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

				// Se reescriben los claims de cognito:groups...
				List<Claim> groupClaims = [.. identity.FindAll("cognito:groups")];
				foreach (Claim claim in groupClaims) {
					identity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
					identity.RemoveClaim(claim);
				}

				// Se desglosan los claims de scopes...
				List<Claim> scopeClaims = [.. identity.FindAll("scope")];
                List<string> scopes = [.. scopeClaims.SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)).Distinct(StringComparer.OrdinalIgnoreCase)];
				foreach (Claim claim in scopeClaims) {
                    identity.RemoveClaim(claim);
                }
				foreach(string scope in scopes) {
                    identity.AddClaim(new Claim("scope", scope));
                }

				return Task.CompletedTask;
			}
		};
	});

builder.Services.AddAuthorizationBuilder()
	.AddPolicy("Admin", policy => policy.RequireRole("Admin"))
	.AddPolicy("Perfil.Read.Self", policy => policy.RequireClaim("scope", "api/perfil.read.self"))
	.AddPolicy("Perfil.Write.Self", policy => policy.RequireClaim("scope", "api/perfil.write.self"))
	.AddPolicy("Obligaciones.Read.Self", policy => policy.RequireClaim("scope", "api/obligaciones.read.self"))
	.AddPolicy("Obligaciones.Write.Self", policy => policy.RequireClaim("scope", "api/obligaciones.write.self"))
	.AddPolicy("Negocios.Read.Self", policy => policy.RequireClaim("scope", "api/negocios.read.self"))
	.AddPolicy("Negocios.Write.Self", policy => policy.RequireClaim("scope", "api/negocios.write.self"))
	.AddPolicy("Vencimientos.Read.Self", policy => policy.RequireClaim("scope", "api/vencimientos.read.self"))
	.AddPolicy("Vencimientos.Write.Self", policy => policy.RequireClaim("scope", "api/vencimientos.write.self"))
	.AddPolicy("Suscripciones.Read.Self", policy => policy.RequireClaim("scope", "api/suscripciones.read.self"))
	.AddPolicy("Suscripciones.Write.Self", policy => policy.RequireClaim("scope", "api/suscripciones.write.self"))
	.AddPolicy("Templates.Read.Public", policy => policy.RequireClaim("scope", "api/templates.read.public"))
	.AddPolicy("Sistema.Read.Public", policy => policy.RequireClaim("scope", "api/sistema.read.public"))
	.AddPolicy("Perfil.Read.All", policy => policy.RequireClaim("scope", "api/perfil.read.all"))
	.AddPolicy("Perfil.Write.All", policy => policy.RequireClaim("scope", "api/perfil.write.all"))
	.AddPolicy("Obligaciones.Read.All", policy => policy.RequireClaim("scope", "api/obligaciones.read.all"))
	.AddPolicy("Obligaciones.Write.All", policy => policy.RequireClaim("scope", "api/obligaciones.write.all"))
	.AddPolicy("Negocios.Read.All", policy => policy.RequireClaim("scope", "api/negocios.read.all"))
	.AddPolicy("Negocios.Write.All", policy => policy.RequireClaim("scope", "api/negocios.write.all"))
	.AddPolicy("Vencimientos.Read.All", policy => policy.RequireClaim("scope", "api/vencimientos.read.all"))
	.AddPolicy("Vencimientos.Write.All", policy => policy.RequireClaim("scope", "api/vencimientos.write.all"))
	.AddPolicy("Templates.Read.All", policy => policy.RequireClaim("scope", "api/templates.read.all"))
	.AddPolicy("Templates.Write.All", policy => policy.RequireClaim("scope", "api/templates.write.all"))
	.AddPolicy("Sistema.Read.All", policy => policy.RequireClaim("scope", "api/sistema.read.all"))
	.AddPolicy("Sistema.Write.All", policy => policy.RequireClaim("scope", "api/sistema.write.all"))
	.AddPolicy("Suscripciones.Read.All", policy => policy.RequireClaim("scope", "api/suscripciones.read.all"))
	.AddPolicy("Suscripciones.Write.All", policy => policy.RequireClaim("scope", "api/suscripciones.write.all"));



WebApplication app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
	app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();
if (!app.Environment.IsDevelopment()) {
	app.UseMiddleware<RateLimitMiddleware>();
}

app.MapCategoriaNormaEndpoints();
app.MapTipoFiscalizadorEndpoints();
app.MapTipoPeriodicidadEndpoints();
app.MapTipoReceptorNotificacionEndpoints();
app.MapTipoUnidadTiempoEndpoints();
app.MapTipoRubroEndpoints();
app.MapTipoActividadEndpoints();
app.MapAuthEndpoints();
app.MapTemplateEndpoints();
app.MapDestinatarioNotificacionEndpoints();
app.MapNegocioEndpoints();
app.MapNormaSuscritaEndpoints();
app.MapInscripcionTemplateEndpoints();
app.MapDocumentoAdjuntoEndpoints();
app.MapMensajeEndpoints();
app.MapWhatsappEndpoints();
app.MapPlanEndpoints();
app.MapSuscripcionEndpoints();
app.MapCargoEndpoints();
app.MapEmpleadoEndpoints();
app.MapPerfilEndpoints();
app.MapPreguntaFrecuenteEndpoints();

await app.RunAsync();

