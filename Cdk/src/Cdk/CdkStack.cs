using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.KMS;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.Route53.Targets;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SecretsManager;
using Amazon.CDK.AWS.SES;
using Amazon.CDK.AWS.SSM;
using Amazon.CDK.AwsApigatewayv2Authorizers;
using Amazon.CDK.AwsApigatewayv2Integrations;
using Amazon.CDK.CustomResources;
using Constructs;
using System;
using System.Collections.Generic;
using System.IO;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;
using CfnStage = Amazon.CDK.AWS.Apigatewayv2.CfnStage;
using CfnStageProps = Amazon.CDK.AWS.Apigatewayv2.CfnStageProps;
using HttpMethod = Amazon.CDK.AWS.Apigatewayv2.HttpMethod;
using IpAddressType = Amazon.CDK.AWS.Apigatewayv2.IpAddressType;
using Secret = Amazon.CDK.AWS.SecretsManager.Secret;

namespace Cdk
{
    public class CdkStack : Stack
    {
        internal CdkStack(Construct scope, string id, CdkStackProps props = null) : base(scope, id, props)
        {
			const string CONST_APP_NAME = "APP_NAME";
			const string CONST_SECRET_ARN = "SECRET_ARN_CONNECTION_STRING";
			
			const string CONST_DIR_RECURSOS = "Recursos";

			const string CONST_ENABLED = "enabled";
			const string CONST_LIGHT_MODE = "lightMode";
			const string CONST_BACKGROUND_COLOR = "backgroundColor";
			const string CONST_BORDER_COLOR = "borderColor";
			const string CONST_DEFAULT = "defaults";
			const string CONST_TEXT_COLOR = "textColor";

			const string CONST_COLOR_MODE = "LIGHT";
			const string CONST_COLOR_CALIPSO = "02b2cbff";
			const string CONST_COLOR_NEGRO = "2a2d34cc";
			const string CONST_COLOR_BLANCO = "ffffffff";

			string appName = System.Environment.GetEnvironmentVariable(CONST_APP_NAME) ?? throw new InvalidOperationException($"No se ha configurado la variable de entorno {CONST_APP_NAME}");
			string regionAws = System.Environment.GetEnvironmentVariable("REGION_AWS") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno REGION_AWS");

			// Para certificado y SES...
			string certDomainName = System.Environment.GetEnvironmentVariable("CERT_DOMAIN_NAME") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno CERT_DOMAIN_NAME");
			string certAlternativeNames = System.Environment.GetEnvironmentVariable("CERT_ALTERNATIVE_NAMES") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno CERT_ALTERNATIVE_NAMES");
			string mailFromDomain = System.Environment.GetEnvironmentVariable("MAIL_FROM_DOMAIN") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno MAIL_FROM_DOMAIN");

			// Para cognito...
			string cognitoCustomDomain = System.Environment.GetEnvironmentVariable("COGNITO_CUSTOM_DOMAIN") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno COGNITO_CUSTOM_DOMAIN");
			
			string[] callbackUrls = System.Environment.GetEnvironmentVariable("CALLBACK_URLS").Split(",") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno CALLBACK_URLS");
			string[] logoutUrls = System.Environment.GetEnvironmentVariable("LOGOUT_URLS").Split(",") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno LOGOUT_URLS");
			string accessTokenValidityMinutes = System.Environment.GetEnvironmentVariable("ACCESS_TOKEN_VALIDITY_MINUTES") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno ACCESS_TOKEN_VALIDITY_MINUTES");
			string idTokenValidityMinutes = System.Environment.GetEnvironmentVariable("ID_TOKEN_VALIDITY_MINUTES") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno ID_TOKEN_VALIDITY_MINUTES");
			string refreshTokenValidityMinutes = System.Environment.GetEnvironmentVariable("REFRESH_TOKEN_VALIDITY_MINUTES") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno REFRESH_TOKEN_VALIDITY_MINUTES");
			string googleOauthClientId = System.Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno GOOGLE_OAUTH_CLIENT_ID");
			string googleOauthClientSecret = System.Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno GOOGLE_OAUTH_CLIENT_SECRET");

			// Para procesos de cognito...
			string cognitoTriggerTokenValidityMinutes = System.Environment.GetEnvironmentVariable("COGNITO_TRIGGER_TOKEN_VALIDITY_MINUTES") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno COGNITO_TRIGGER_TOKEN_VALIDITY_MINUTES");
			string arnParameterCognitoTriggerLambdaArn = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_COGNITO_TRIGGER_LAMBDA_ARN") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno ARN_PARAMETER_COGNITO_TRIGGER_LAMBDA_ARN");

			// Para procesos de notificación...
			string notificacionesTokenValidityMinutes = System.Environment.GetEnvironmentVariable("NOTIFICACIONES_TOKEN_VALIDITY_MINUTES") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno NOTIFICACIONES_TOKEN_VALIDITY_MINUTES");

			// Para infraestructura...
			string publishZip = System.Environment.GetEnvironmentVariable("PUBLISH_ZIP") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno PUBLISH_ZIP");
            string handler = System.Environment.GetEnvironmentVariable("HANDLER") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno HANDLER");
            string timeout = System.Environment.GetEnvironmentVariable("TIMEOUT") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno TIMEOUT");
            string memorySize = System.Environment.GetEnvironmentVariable("MEMORY_SIZE") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno MEMORY_SIZE");
            string domainName = System.Environment.GetEnvironmentVariable("DOMAIN_NAME") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno DOMAIN_NAME");
            string vpcId = System.Environment.GetEnvironmentVariable("VPC_ID") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno VPC_ID");
            string privateWithInternetId1 = System.Environment.GetEnvironmentVariable("PRIVATE_WITH_INTERNET_ID_1") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno PRIVATE_WITH_INTERNET_ID_1");
            string privateWithInternetId2 = System.Environment.GetEnvironmentVariable("PRIVATE_WITH_INTERNET_ID_2") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno PRIVATE_WITH_INTERNET_ID_2");
            string rdsSecurityGroupId = System.Environment.GetEnvironmentVariable("RDS_SECURITY_GROUP_ID") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno RDS_SECURITY_GROUP_ID");

            // Variables de entorno de la lambda...
            string secretArnConnectionString = System.Environment.GetEnvironmentVariable(CONST_SECRET_ARN) ?? throw new InvalidOperationException($"No se ha configurado la variable de entorno {CONST_SECRET_ARN}");
            string allowedDomains = System.Environment.GetEnvironmentVariable("ALLOWED_DOMAINS") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno ALLOWED_DOMAINS");
			string arnParameterHermesApiUrl = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_HERMES_API_URL") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno ARN_PARAMETER_HERMES_API_URL");
			string arnParameterHermesApiKeyId = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_HERMES_API_KEY_ID") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno ARN_PARAMETER_HERMES_API_KEY_ID");
			string hermesDeNombre = System.Environment.GetEnvironmentVariable("HERMES_DE_NOMBRE") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno HERMES_DE_NOMBRE");
			string hermesDeCorreo = System.Environment.GetEnvironmentVariable("HERMES_DE_CORREO") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno HERMES_DE_CORREO");
			string hermesDeWhatsapp = System.Environment.GetEnvironmentVariable("HERMES_DE_WHATSAPP") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno HERMES_DE_WHATSAPP");
			string arnParameterKairosApiUrl = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_KAIROS_API_URL") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno ARN_PARAMETER_KAIROS_API_URL");
			string arnParameterKairosApiKeyId = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_KAIROS_API_KEY_ID") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno ARN_PARAMETER_KAIROS_API_KEY_ID");
			string arnParameterNotificacionesLambdaArn = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_NOTIFICACIONES_LAMBDA_ARN") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno ARN_PARAMETER_NOTIFICACIONES_LAMBDA_ARN");
			string arnParameterNotificacionesEjecucionRoleArn = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_NOTIFICACIONES_EJECUCION_ROLE_ARN") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno ARN_PARAMETER_NOTIFICACIONES_EJECUCION_ROLE_ARN");

			string googleOAuth2ApiUrl = System.Environment.GetEnvironmentVariable("GOOGLE_OAUTH2_API_URL") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno GOOGLE_OAUTH2_API_URL");
			string googleOAuth2Scope = System.Environment.GetEnvironmentVariable("GOOGLE_OAUTH2_SCOPE") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno GOOGLE_OAUTH2_SCOPE");
			string googleOAuth2GrantType = System.Environment.GetEnvironmentVariable("GOOGLE_OAUTH2_GRANT_TYPE") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno GOOGLE_OAUTH2_GRANT_TYPE");
			string googleRecaptchaApiUrl = System.Environment.GetEnvironmentVariable("GOOGLE_RECAPTCHA_API_URL") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno GOOGLE_RECAPTCHA_API_URL");
			string googleRecaptchaCredential = System.Environment.GetEnvironmentVariable("GOOGLE_RECAPTCHA_CREDENTIAL") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno GOOGLE_RECAPTCHA_CREDENTIAL");
			string googleRecaptchaProjectId = System.Environment.GetEnvironmentVariable("GOOGLE_RECAPTCHA_PROJECT_ID") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno GOOGLE_RECAPTCHA_PROJECT_ID");
			string googleRecaptchaSiteKey = System.Environment.GetEnvironmentVariable("GOOGLE_RECAPTCHA_SITE_KEY") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno GOOGLE_RECAPTCHA_SITE_KEY");
			string destinatariosNuevoMensaje = System.Environment.GetEnvironmentVariable("DESTINATARIOS_NUEVO_MENSAJE") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno DESTINATARIOS_NUEVO_MENSAJE");
			string flowApiKey = System.Environment.GetEnvironmentVariable("FLOW_API_KEY") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno FLOW_API_KEY");
			string flowSecretKey = System.Environment.GetEnvironmentVariable("FLOW_SECRET_KEY") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno FLOW_SECRET_KEY");
			string flowApiUrl = System.Environment.GetEnvironmentVariable("FLOW_API_URL") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno FLOW_API_URL");
			string flowUrlCallback = System.Environment.GetEnvironmentVariable("FLOW_URL_CALLBACK") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno FLOW_URL_CALLBACK");
			string flowUrlRetorno = System.Environment.GetEnvironmentVariable("FLOW_URL_RETORNO") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno FLOW_URL_RETORNO");
            string urlCodigoVerificacion = System.Environment.GetEnvironmentVariable("URL_CODIGO_VERIFICACION") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno URL_CODIGO_VERIFICACION");
            
            // Variables de entorno para la lambda de ejecución inicial...
            string appSchemaName = System.Environment.GetEnvironmentVariable("APP_SCHEMA_NAME") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno APP_SCHEMA_NAME");
            string initialCreationHandler = System.Environment.GetEnvironmentVariable("INITIAL_CREATION_HANDLER") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno INITIAL_CREATION_HANDLER");
            string initialCreationPublishZip = System.Environment.GetEnvironmentVariable("INITIAL_CREATION_PUBLISH_ZIP") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno INITIAL_CREATION_PUBLISH_ZIP");
            string migrationScript = System.Environment.GetEnvironmentVariable("MIGRATION_SCRIPT") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno MIGRATION_SCRIPT");

			#region Certificado, Dominio API Gateway y SES
			// Se crea certificado...		
			Certificate certificate = new(this, $"{appName}Certificate", new CertificateProps {
				CertificateName = $"{appName}Certificate",
				DomainName = certDomainName,
				SubjectAlternativeNames = certAlternativeNames.Split(","),
				Validation = CertificateValidation.FromDns(props.HostedZone),
			});

			// Se crea el dominio al API Gateway
			DomainName apiGatewayDomain = new(this, $"{appName}DomainName", new DomainNameProps {
				DomainName = domainName,
				Certificate = certificate,
				EndpointType = EndpointType.REGIONAL,
				IpAddressType = IpAddressType.DUAL_STACK
			});

			// Se crea el ARecord para el subdominio del API Gateway
			_ = new ARecord(this, $"{appName}ApiGatewayARecord", new ARecordProps {
				Zone = props.HostedZone,
				RecordName = domainName,
				Target = RecordTarget.FromAlias(new ApiGatewayv2DomainProperties(apiGatewayDomain.RegionalDomainName, apiGatewayDomain.RegionalHostedZoneId))
			});

			_ = new AaaaRecord(this, $"{appName}ApiGatewayAAAARecord", new AaaaRecordProps {
				Zone = props.HostedZone,
				RecordName = domainName,
				Target = RecordTarget.FromAlias(new ApiGatewayv2DomainProperties(apiGatewayDomain.RegionalDomainName, apiGatewayDomain.RegionalHostedZoneId))
			});

			IPublicHostedZone publicHostedZone = PublicHostedZone.FromPublicHostedZoneAttributes(this, $"{appName}PublicHostedZone", new PublicHostedZoneAttributes {
				ZoneName = props.HostedZone.ZoneName,
				HostedZoneId = props.HostedZone.HostedZoneId,
			});

			// Se crea email identity para envío de correos...
			_ = new EmailIdentity(this, $"{appName}EmailIdentity", new EmailIdentityProps {
				Identity = Identity.PublicHostedZone(publicHostedZone),
				MailFromDomain = mailFromDomain,
				MailFromBehaviorOnMxFailure = MailFromBehaviorOnMxFailure.USE_DEFAULT_VALUE,
			});
			#endregion

			// Se obtiene la VPC y subnets...
			IVpc vpc = Vpc.FromLookup(this, $"{appName}Vpc", new VpcLookupOptions {
                VpcId = vpcId
            });

            ISubnet subnet1 = Subnet.FromSubnetId(this, $"{appName}Subnet1", privateWithInternetId1);
            ISubnet subnet2 = Subnet.FromSubnetId(this, $"{appName}Subnet2", privateWithInternetId2);

			// Se busca Lambda Function para procesar PostConfirmation...
			IStringParameter cognitoTriggerLambdaArnStringParameter =  StringParameter.FromStringParameterArn(this, $"{appName}CognitoTriggerLambdaArnStringParameter", arnParameterCognitoTriggerLambdaArn);
			IFunction postConfirmationFunction = Function.FromFunctionAttributes(this, $"{appName}CognitoTriggerLambda", new FunctionAttributes { 
				FunctionArn = cognitoTriggerLambdaArnStringParameter.StringValue,
				SameEnvironment = true,
			});

			#region Cognito
			Key kmsKey = new(this, $"{appName}KMSKey", new KeyProps {
				Description = $"KMS Key para aplicación {appName}",
				RemovalPolicy = RemovalPolicy.DESTROY,
			});

            UserPool userPool = new(this, $"{appName}UserPool", new UserPoolProps {
				UserPoolName = $"{appName}UserPool",
				SelfSignUpEnabled = true,
				SignInCaseSensitive = false,
				UserVerification = new UserVerificationConfig {
					EmailStyle = VerificationEmailStyle.CODE,
				},
				CustomSenderKmsKey = kmsKey,
				SignInAliases = new SignInAliases {
					Username = false,
					Email = true,
				},
				AutoVerify = new AutoVerifiedAttrs {
					Email = true,
				},
				KeepOriginal = new KeepOriginalAttrs {
					Email = true,
				},
				Mfa = Mfa.OPTIONAL,
				MfaSecondFactor = new MfaSecondFactor {
					Otp = true,
				},
				AccountRecovery = AccountRecovery.EMAIL_ONLY,
				StandardAttributes = new StandardAttributes {
					Email = new StandardAttribute {
						Required = true,
						Mutable = true,
					},
					GivenName = new StandardAttribute {
						Required = true,
						Mutable = true,
					},
					FamilyName = new StandardAttribute {
						Required = true,
						Mutable = true,
					},
				},
				PasswordPolicy = new PasswordPolicy {
					MinLength = 8,
					RequireLowercase = true,
					RequireUppercase = true,
					RequireDigits = true,
					RequireSymbols = false,
				},
				DeletionProtection = true,
				LambdaTriggers = new UserPoolTriggers {
					PostConfirmation = postConfirmationFunction,
					CustomEmailSender = postConfirmationFunction,
				}
			});
			
			_ = new UserPoolGroup(this, $"{appName}AdminUserGroup", new UserPoolGroupProps {
				GroupName = "Admin",
				UserPool = userPool,
				Description = $"Administrador de la aplicacion {appName}",
			});

			UserPoolDomain userPoolDomain = new(this, $"{appName}CognitoDomain", new UserPoolDomainProps {
				UserPool = userPool,
				CustomDomain = new CustomDomainOptions {
					DomainName = cognitoCustomDomain,
					Certificate = props.Certificate,
				},
				ManagedLoginVersion = ManagedLoginVersion.NEWER_MANAGED_LOGIN,
			});

			UserPoolIdentityProviderGoogle googleProvider = new(this, $"{appName}IdentityProviderGoogle", new UserPoolIdentityProviderGoogleProps {
				UserPool = userPool,
				ClientId = googleOauthClientId,
				ClientSecretValue = SecretValue.UnsafePlainText(googleOauthClientSecret),
				Scopes = ["email", "profile"],
				AttributeMapping = new AttributeMapping() {
					Email = ProviderAttribute.GOOGLE_EMAIL,
					GivenName = ProviderAttribute.GOOGLE_GIVEN_NAME,
					FamilyName = ProviderAttribute.GOOGLE_FAMILY_NAME,
				}
			});

			// Se crean scopes y resource server...
			// Formato: <entidad>.<accion>.<alcance>
			// Alcances:
			//     - self: Datos del usuario
			//     - all: Todos los datos del sistema
			//     - public: Datos públicos / vigentes
			// Acción:
			//     - read: Lectura
			//     - write: Crear, editar y borrar

			#region Self Scopes
			ResourceServerScope scopePerfilReadSelf = new(new ResourceServerScopeProps {
				ScopeName = "perfil.read.self",
				ScopeDescription = "Acceso de lectura al perfil del usuario"
			});
			ResourceServerScope scopePerfilWriteSelf = new(new ResourceServerScopeProps {
				ScopeName = "perfil.write.self",
				ScopeDescription = "Acceso de escritura al perfil del usuario"
			});

			ResourceServerScope scopeObligacionesReadSelf = new(new ResourceServerScopeProps {
				ScopeName = "obligaciones.read.self",
				ScopeDescription = "Acceso de lectura a las obligaciones del usuario"
			});
			ResourceServerScope scopeObligacionesWriteSelf = new(new ResourceServerScopeProps {
				ScopeName = "obligaciones.write.self",
				ScopeDescription = "Acceso de escritura a las obligaciones del usuario"
            });

            ResourceServerScope scopeNegociosReadSelf = new(new ResourceServerScopeProps {
                ScopeName = "negocios.read.self",
                ScopeDescription = "Acceso de lectura a los negocios del usuario"
            });
            ResourceServerScope scopeNegociosWriteSelf = new(new ResourceServerScopeProps {
                ScopeName = "negocios.write.self",
                ScopeDescription = "Acceso de escritura a los negocios del usuario"
            });

            ResourceServerScope scopeVencimientosReadSelf = new(new ResourceServerScopeProps {
                ScopeName = "vencimientos.read.self",
                ScopeDescription = "Acceso de lectura a los vencimientos del usuario"
            });
            ResourceServerScope scopeVencimientosWriteSelf = new(new ResourceServerScopeProps {
                ScopeName = "vencimientos.write.self",
                ScopeDescription = "Acceso de escritura a los vencimientos del usuario"
            });

			ResourceServerScope scopeSuscripcionesReadSelf = new(new ResourceServerScopeProps {
				ScopeName = "suscripciones.read.self",
				ScopeDescription = "Acceso de lectura a las suscripciones del usuario"
			});
			ResourceServerScope scopeSuscripcionesWriteSelf = new(new ResourceServerScopeProps {
				ScopeName = "suscripciones.write.self",
				ScopeDescription = "Acceso de escritura a las suscripciones del usuario"
			});
			#endregion

			#region Public Scopes
			ResourceServerScope scopeTemplatesReadPublic = new(new ResourceServerScopeProps {
				ScopeName = "templates.read.public",
				ScopeDescription = "Acceso de lectura a los templates públicos"
			});

			ResourceServerScope scopeSistemaReadPublic = new(new ResourceServerScopeProps {
				ScopeName = "sistema.read.public",
				ScopeDescription = "Acceso de lectura a los parametros públicos del sistema"
            });
			#endregion

			#region All Scopes
			ResourceServerScope scopePerfilReadAll = new(new ResourceServerScopeProps {
				ScopeName = "perfil.read.all",
				ScopeDescription = "Acceso de lectura a todas los perfiles"
			});
			ResourceServerScope scopePerfilWriteAll = new(new ResourceServerScopeProps {
				ScopeName = "perfil.write.all",
				ScopeDescription = "Acceso de escritura a todas los perfiles"
			});

			ResourceServerScope scopeObligacionesReadAll = new(new ResourceServerScopeProps {
                ScopeName = "obligaciones.read.all",
                ScopeDescription = "Acceso de lectura a todas las obligaciones"
            });
            ResourceServerScope scopeObligacionesWriteAll = new(new ResourceServerScopeProps {
                ScopeName = "obligaciones.write.all",
                ScopeDescription = "Acceso de escritura a todas las obligaciones"
            });

            ResourceServerScope scopeNegociosReadAll = new(new ResourceServerScopeProps {
                ScopeName = "negocios.read.all",
                ScopeDescription = "Acceso de lectura a todos los negocios"
            });
            ResourceServerScope scopeNegociosWriteAll = new(new ResourceServerScopeProps {
                ScopeName = "negocios.write.all",
                ScopeDescription = "Acceso de escritura a todos los negocios"
            });

            ResourceServerScope scopeVencimientosReadAll = new(new ResourceServerScopeProps {
                ScopeName = "vencimientos.read.all",
                ScopeDescription = "Acceso de lectura a todos los vencimientos"
            });
            ResourceServerScope scopeVencimientosWriteAll = new(new ResourceServerScopeProps {
                ScopeName = "vencimientos.write.all",
                ScopeDescription = "Acceso de escritura a todos los vencimientos"
            });

            ResourceServerScope scopeTemplatesReadAll = new(new ResourceServerScopeProps {
                ScopeName = "templates.read.all",
                ScopeDescription = "Acceso de lectura a todos los templates"
            });
            ResourceServerScope scopeTemplatesWriteAll = new(new ResourceServerScopeProps {
                ScopeName = "templates.write.all",
                ScopeDescription = "Acceso de escritura a todos los templates"
            });

            ResourceServerScope scopeSistemaReadAll = new(new ResourceServerScopeProps {
                ScopeName = "sistema.read.all",
                ScopeDescription = "Acceso de lectura a todos los parametros del sistema"
            });
            ResourceServerScope scopeSistemaWriteAll = new(new ResourceServerScopeProps {
                ScopeName = "sistema.write.all",
                ScopeDescription = "Acceso de escritura a todos los parametros del sistema"
            });

			ResourceServerScope scopeSuscripcionesReadAll = new(new ResourceServerScopeProps {
				ScopeName = "suscripciones.read.all",
				ScopeDescription = "Acceso de lectura a todas las suscripciones"
			});
			ResourceServerScope scopeSuscripcionesWriteAll = new(new ResourceServerScopeProps {
				ScopeName = "suscripciones.write.all",
				ScopeDescription = "Acceso de escritura a todas las suscripciones"
			});
			#endregion


			UserPoolResourceServer resourceServer =  userPool.AddResourceServer($"{appName}ResourceServer", new UserPoolResourceServerOptions { 
				Identifier = "api",
				Scopes = [
					scopePerfilReadSelf,
					scopePerfilWriteSelf,
					scopeObligacionesReadSelf,
					scopeObligacionesWriteSelf,
					scopeNegociosReadSelf,
                    scopeNegociosWriteSelf,
                    scopeVencimientosReadSelf,
                    scopeVencimientosWriteSelf,
					scopeSuscripcionesReadSelf,
					scopeSuscripcionesWriteSelf,
                    scopeTemplatesReadPublic,
                    scopeSistemaReadPublic,
					scopePerfilReadAll,
					scopePerfilWriteAll,
					scopeObligacionesReadAll,
                    scopeObligacionesWriteAll,
                    scopeNegociosReadAll,
                    scopeNegociosWriteAll,
                    scopeVencimientosReadAll,
                    scopeVencimientosWriteAll,
                    scopeTemplatesReadAll,
                    scopeTemplatesWriteAll,
                    scopeSistemaReadAll,
                    scopeSistemaWriteAll,
					scopeSuscripcionesReadAll,
					scopeSuscripcionesWriteAll,
                ]
			});

			UserPoolClient userPoolClient = new(this, $"{appName}UserPoolClient", new UserPoolClientProps {
				UserPoolClientName = $"{appName}UserPoolClient",
				UserPool = userPool,
				GenerateSecret = false,
				PreventUserExistenceErrors = true,
				AuthFlows = new AuthFlow {
					UserSrp = true,
				},
				SupportedIdentityProviders = [
					UserPoolClientIdentityProvider.COGNITO,
					UserPoolClientIdentityProvider.GOOGLE,
                ],
				OAuth = new OAuthSettings {
					CallbackUrls = callbackUrls,
					LogoutUrls = logoutUrls,
					Flows = new OAuthFlows { AuthorizationCodeGrant = true },
					Scopes = [
						OAuthScope.OPENID, OAuthScope.EMAIL, OAuthScope.PROFILE,
						OAuthScope.ResourceServer(resourceServer, scopePerfilReadSelf),
						OAuthScope.ResourceServer(resourceServer, scopePerfilWriteSelf),
						OAuthScope.ResourceServer(resourceServer, scopeObligacionesReadSelf),
						OAuthScope.ResourceServer(resourceServer, scopeObligacionesWriteSelf),
						OAuthScope.ResourceServer(resourceServer, scopeNegociosReadSelf),
						OAuthScope.ResourceServer(resourceServer, scopeNegociosWriteSelf),
						OAuthScope.ResourceServer(resourceServer, scopeVencimientosReadSelf),
						OAuthScope.ResourceServer(resourceServer, scopeVencimientosWriteSelf),
						OAuthScope.ResourceServer(resourceServer, scopeSuscripcionesReadSelf),
						OAuthScope.ResourceServer(resourceServer, scopeSuscripcionesWriteSelf),
						OAuthScope.ResourceServer(resourceServer, scopeTemplatesReadPublic),
						OAuthScope.ResourceServer(resourceServer, scopeSistemaReadPublic),
					]
				},
				AccessTokenValidity = Duration.Minutes(double.Parse(accessTokenValidityMinutes)),
				IdTokenValidity = Duration.Minutes(double.Parse(idTokenValidityMinutes)),
				RefreshTokenValidity = Duration.Minutes(double.Parse(refreshTokenValidityMinutes))
			});
			userPoolClient.Node.AddDependency(googleProvider);

			// Se crea userpoolclient a ser usado por aplicacion de notificaciones...
			UserPoolClient notificacionesUserPoolClient = new(this, $"{appName}NotificacionesUserPoolClient", new UserPoolClientProps {
				UserPoolClientName = $"{appName}NotificacionesUserPoolClient",
				UserPool = userPool,
				GenerateSecret = true,
				AuthFlows = new AuthFlow {
					AdminUserPassword = false,
					UserPassword = false,
					UserSrp = false,
				},
				SupportedIdentityProviders = [
					UserPoolClientIdentityProvider.COGNITO
				],
				OAuth = new OAuthSettings {
					Flows = new OAuthFlows { ClientCredentials = true },
					Scopes = [
						OAuthScope.ResourceServer(resourceServer, scopeObligacionesReadAll),
						OAuthScope.ResourceServer(resourceServer, scopeNegociosReadAll),
						OAuthScope.ResourceServer(resourceServer, scopeVencimientosReadAll),
						OAuthScope.ResourceServer(resourceServer, scopeVencimientosWriteAll),
                        OAuthScope.ResourceServer(resourceServer, scopeTemplatesReadPublic),
                        OAuthScope.ResourceServer(resourceServer, scopeSistemaReadPublic),

                    ]
				},
				AccessTokenValidity = Duration.Minutes(double.Parse(notificacionesTokenValidityMinutes))
			});

			// Se crea userpoolclient a ser usado por aplicación de cognito...
			UserPoolClient cognitoTriggerUserPoolClient = new(this, $"{appName}CognitoTriggerUserPoolClient", new UserPoolClientProps {
				UserPoolClientName = $"{appName}CognitoTriggerUserPoolClient",
				UserPool = userPool,
				GenerateSecret = true,
				AuthFlows = new AuthFlow {
					AdminUserPassword = false,
					UserPassword = false,
					UserSrp = false,
				},
				SupportedIdentityProviders = [
					UserPoolClientIdentityProvider.COGNITO
				],
				OAuth = new OAuthSettings {
					Flows = new OAuthFlows { ClientCredentials = true },
					Scopes = [
						OAuthScope.ResourceServer(resourceServer, scopePerfilReadAll),
						OAuthScope.ResourceServer(resourceServer, scopePerfilWriteAll),
						OAuthScope.ResourceServer(resourceServer, scopeSuscripcionesReadAll),
						OAuthScope.ResourceServer(resourceServer, scopeSuscripcionesWriteAll),
						OAuthScope.ResourceServer(resourceServer, scopeSistemaReadPublic),

					]
				},
				AccessTokenValidity = Duration.Minutes(double.Parse(cognitoTriggerTokenValidityMinutes))
			});

			string base64Favicon = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONST_DIR_RECURSOS, "FAVICON.ico")));
			string base64PageHeaderLogo = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONST_DIR_RECURSOS, "PAGE_HEADER_LOGO.svg")));
			string base64PageFooterLogo = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONST_DIR_RECURSOS, "PAGE_FOOTER_LOGO.svg")));
			string base64BackgroundImage = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONST_DIR_RECURSOS, "BACKGROUND_IMAGE.jpeg")));

			_ = new CfnManagedLoginBranding(this, $"{appName}ManagedLoginBranding", new CfnManagedLoginBrandingProps {
				UserPoolId = userPool.UserPoolId,
				ClientId = userPoolClient.UserPoolClientId,
				ReturnMergedResources = true,
				Settings = new Dictionary<string, object> {
					{ "categories", new Dictionary<string, object> {
						{ "form", new Dictionary<string, object> {
							{ "languageSelector", new Dictionary<string, object> {
								{ CONST_ENABLED, true }
							}}
						}},
						{ "global", new Dictionary<string, object> {
							{ "colorSchemeMode", CONST_COLOR_MODE },
							{ "pageHeader", new Dictionary<string, object> {
								{ CONST_ENABLED, true }
							}},
							{ "pageFooter", new Dictionary<string, object> {
								{ CONST_ENABLED, true }
							}}
						}}
					}},
					{ "componentClasses", new Dictionary<string, object>{
						{ "buttons", new Dictionary<string, object>{
							{ "borderRadius", 16.0 }
						}},
						{ "optionControls", new Dictionary<string, object>{
							{ CONST_LIGHT_MODE, new Dictionary<string, object> {
								{ "selected", new Dictionary<string, object>{
									{ CONST_BACKGROUND_COLOR, "005db2ff" }
								}},
							}}
						}},
						{ "focusState", new Dictionary<string, object>{
							{ CONST_LIGHT_MODE, new Dictionary<string, object> {
								{ CONST_BORDER_COLOR, CONST_COLOR_CALIPSO }
							}}
						}},
						{ "input", new Dictionary<string, object>{
							{ CONST_LIGHT_MODE, new Dictionary<string, object> {
								{ CONST_DEFAULT, new Dictionary<string, object>{
                                    { CONST_BORDER_COLOR, CONST_COLOR_NEGRO }
                                }},
								{ "placeholderColor", "2a2d34b3" },
							}}
						}},
						{ "inputLabel", new Dictionary<string, object>{
							{ CONST_LIGHT_MODE, new Dictionary<string, object> {
								{ CONST_TEXT_COLOR, CONST_COLOR_NEGRO }
                            }}
						}},
						{ "link", new Dictionary<string, object>{
							{ CONST_LIGHT_MODE, new Dictionary<string, object> {
								{ CONST_DEFAULT, new Dictionary<string, object>{
									{ CONST_TEXT_COLOR, "005db2ff" }
								}},
								{ "hover", new Dictionary<string, object>{
									{ CONST_TEXT_COLOR, "005db2cc" }
								}},
							}}
						}}
					}},
					{ "components", new Dictionary<string, object>{
						{ "favicon", new Dictionary<string, object> {
							{ "enabledTypes", new string[1] { "ICO" }},
						}},
						{ "pageHeader", new Dictionary<string, object> {
							{ "backgroundImage", new Dictionary<string, object> {
								{ CONST_ENABLED, false }
							}},
							{ "logo", new Dictionary<string, object> {
								{ CONST_ENABLED, true },
								{ "location", "START" }
							}},
							{ CONST_LIGHT_MODE, new Dictionary<string, object> {
								{ "background", new Dictionary<string, object>{
                                    { "color", CONST_COLOR_BLANCO }
                                }},
								{ CONST_BORDER_COLOR, "f5f5f5ff" }
							}}
						}},
						{ "pageFooter", new Dictionary<string, object> {
							{ "backgroundImage", new Dictionary<string, object> {
								{ CONST_ENABLED, false }
							}},
							{ "logo", new Dictionary<string, object> {
								{ CONST_ENABLED, true },
								{ "location", "START" }
							}},
							{ CONST_LIGHT_MODE, new Dictionary<string, object> {
								{ "background", new Dictionary<string, object>{
									{ "color", "2a2d34ff" }
								}},
								{ CONST_BORDER_COLOR, "2a2d34ff" }
							}}
						}},
						{ "form", new Dictionary<string, object> {
							{ "borderRadius", 0.0 },
							{ "logo", new Dictionary<string, object> {
								{ CONST_ENABLED, false },
							}},
							{ CONST_LIGHT_MODE, new Dictionary<string, object> {
								{ CONST_BACKGROUND_COLOR, CONST_COLOR_BLANCO },
								{ CONST_BORDER_COLOR, CONST_COLOR_BLANCO },
							}}
						}},
						{ "pageBackground", new Dictionary<string, object> {
							{ "image", new Dictionary<string, object> {
								{ CONST_ENABLED, true }
							}},
						}},
						{ "pageText", new Dictionary<string, object> {
							{ CONST_LIGHT_MODE, new Dictionary<string, object> {
								{ "headingColor", CONST_COLOR_NEGRO },
								{ "bodyColor", CONST_COLOR_NEGRO },
								{ "descriptionColor", CONST_COLOR_NEGRO },
							}},
						}},
						{ "primaryButton", new Dictionary<string, object> {
							{ CONST_LIGHT_MODE, new Dictionary<string, object> {
								{ CONST_DEFAULT, new Dictionary<string, object>{
									{ CONST_BACKGROUND_COLOR, CONST_COLOR_CALIPSO },
									{ CONST_TEXT_COLOR, CONST_COLOR_BLANCO }
								}},
								{ "hover", new Dictionary<string, object>{
									{ CONST_BACKGROUND_COLOR, "02b2cbcc" },
									{ CONST_TEXT_COLOR, CONST_COLOR_BLANCO }
								}},
								{ "active", new Dictionary<string, object>{
									{ CONST_BACKGROUND_COLOR, CONST_COLOR_CALIPSO },
									{ CONST_TEXT_COLOR, CONST_COLOR_BLANCO }
								}},
							}},
						}},
						{ "secondaryButton", new Dictionary<string, object> {
							{ CONST_LIGHT_MODE, new Dictionary<string, object> {
								{ CONST_DEFAULT, new Dictionary<string, object>{
									{ CONST_BACKGROUND_COLOR, CONST_COLOR_BLANCO },
									{ CONST_BORDER_COLOR, CONST_COLOR_CALIPSO },
									{ CONST_TEXT_COLOR, CONST_COLOR_CALIPSO }
								}},
								{ "hover", new Dictionary<string, object>{
									{ CONST_BACKGROUND_COLOR, CONST_COLOR_BLANCO },
									{ CONST_BORDER_COLOR, "02b2cbcc" },
									{ CONST_TEXT_COLOR, CONST_COLOR_CALIPSO }
								}},
								{ "active", new Dictionary<string, object>{
									{ CONST_BACKGROUND_COLOR, CONST_COLOR_BLANCO },
									{ CONST_BORDER_COLOR, CONST_COLOR_CALIPSO },
									{ CONST_TEXT_COLOR, CONST_COLOR_CALIPSO }
								}},
							}},
						}}
					}}
				},
				Assets = (new List<CfnManagedLoginBranding.AssetTypeProperty>() {
					new() {
						Category = "PAGE_HEADER_LOGO",
						ColorMode = CONST_COLOR_MODE,
						Extension = "SVG",
						Bytes = base64PageHeaderLogo,
					},
					new() {
						Category = "PAGE_FOOTER_LOGO",
						ColorMode = CONST_COLOR_MODE,
						Extension = "SVG",
						Bytes = base64PageFooterLogo,
					},
					new() {
						Category = "PAGE_BACKGROUND",
						ColorMode = CONST_COLOR_MODE,
						Extension = "JPEG",
						Bytes = base64BackgroundImage,
					},
					new() {
						Category = "FAVICON_ICO",
						ColorMode = CONST_COLOR_MODE,
						Extension = "ICO",
						Bytes = base64Favicon,
					},
				}).ToArray()
			});

			// Se crea record en hosted zone...
			_ = new ARecord(this, $"{appName}LoginARecord", new ARecordProps {
				Zone = props.HostedZone,
				RecordName = cognitoCustomDomain,
				Target = RecordTarget.FromAlias(new UserPoolDomainTarget(userPoolDomain)),
			});

			_ = new AaaaRecord(this, $"{appName}LoginAAAARecord", new AaaaRecordProps {
				Zone = props.HostedZone,
				RecordName = cognitoCustomDomain,
				Target = RecordTarget.FromAlias(new UserPoolDomainTarget(userPoolDomain)),
			});

			// Se configuran parámetros para ser rescatados por consumidores...
			Secret secret = new(this, $"{appName}Secret", new SecretProps {
                SecretName = $"/{appName}",
                Description = $"Secretos de la aplicacion de {appName}",
                SecretObjectValue = new Dictionary<string, SecretValue> {
                    { "CognitoBaseUrl", SecretValue.UnsafePlainText(userPoolDomain.BaseUrl()) },
                    { "NotificacionesUserPoolClientId", SecretValue.UnsafePlainText(notificacionesUserPoolClient.UserPoolClientId) },
                    { "NotificacionesUserPoolClientSecret", notificacionesUserPoolClient.UserPoolClientSecret },
					{ "CognitoTriggerUserPoolClientId", SecretValue.UnsafePlainText(cognitoTriggerUserPoolClient.UserPoolClientId) },
					{ "CognitoTriggerUserPoolClientSecret", cognitoTriggerUserPoolClient.UserPoolClientSecret },
					{ "GoogleRecaptchaCredential", SecretValue.UnsafePlainText(googleRecaptchaCredential) },
					{ "FlowApiKey", SecretValue.UnsafePlainText(flowApiKey) },
					{ "FlowSecretKey", SecretValue.UnsafePlainText(flowSecretKey) },
				},
            });
			#endregion

			#region S3
			Bucket bucket = new(this, $"{appName}BucketDocumentosAdjuntos", new BucketProps {
				BucketName = $"{appName.ToLowerInvariant()}-documentos-adjuntos",
				BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
				Versioned = true,
				EnforceSSL = true,
				Cors = [
					new CorsRule {
						AllowedHeaders = ["*"],
						AllowedOrigins = allowedDomains.Split(","),
						AllowedMethods = [
							HttpMethods.GET,
							HttpMethods.PUT,
							HttpMethods.POST,
						],
						MaxAge = 10 * 24 * 60 * 60
					}
				],
				LifecycleRules = [
					new LifecycleRule { 
						Id = "MoverADeepArchiveCuandoEliminado",
						Enabled = true,
						Transitions = [
							new Transition {
								StorageClass = StorageClass.DEEP_ARCHIVE,
								TransitionAfter = Duration.Days(0)
							}
						],
						TagFilters = new Dictionary<string, object> {
							{ "Estado", "Eliminado" }
						}
					}	
				],
				RemovalPolicy = RemovalPolicy.DESTROY,
				AutoDeleteObjects = false,
			});
            #endregion

            #region DynamoDB para rate limits
            Table tablaRateLimits = new(this, $"{appName}DynamoDBTableRateLimits", new TableProps {
                TableName = $"{appName}RateLimits",
                PartitionKey = new Attribute {
                    Name = "PK",
                    Type = AttributeType.STRING
                },
                SortKey = new Attribute {
                    Name = "SK",
                    Type = AttributeType.STRING
                },
				TimeToLiveAttribute = "TTL",
                DeletionProtection = true,
                BillingMode = BillingMode.PAY_PER_REQUEST,
                RemovalPolicy = RemovalPolicy.DESTROY
            });
            #endregion

            #region API
            // Se crea security group para la lambda y se enlaza con security group de RDS...
            SecurityGroup securityGroup = new(this, $"{appName}LambdaSecurityGroup", new SecurityGroupProps {
                Vpc = vpc,
                SecurityGroupName = $"{appName}APILambda",
                Description = $"Security Group de {appName} API Lambda",
                AllowAllOutbound = true,
            });

            ISecurityGroup rdsSecurityGroup = SecurityGroup.FromSecurityGroupId(this, $"{appName}RDSSecurityGroup", rdsSecurityGroupId);
            rdsSecurityGroup.AddIngressRule(securityGroup, Port.POSTGRES, $"Allow connection from {appName} API Lambda to RDS");

            // Creación de log group lambda...
            LogGroup logGroup = new(this, $"{appName}APILogGroup", new LogGroupProps {
                LogGroupName = $"/aws/lambda/{appName}API/logs",
                Retention = RetentionDays.ONE_MONTH,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

			// Se obtienen parámetros usados por la lambda...
			IStringParameter parameterHermesApiUrl = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterHermesApiUrl", arnParameterHermesApiUrl);
			IStringParameter parameterHermesApiKeyId = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterHermesApiKeyId", arnParameterHermesApiKeyId);
			IStringParameter parameterKairosApiUrl = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterKairosApiUrl", arnParameterKairosApiUrl);
			IStringParameter parameterKairosApiKeyId = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterKairosApiKeyId", arnParameterKairosApiKeyId);
			IStringParameter parameterNotificacionesLambdaArn = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterNotificacionesLambdaArn", arnParameterNotificacionesLambdaArn);
			IStringParameter parameterNotificacionesEjecucionRoleArn = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterNotificacionesEjecucionRoleArn", arnParameterNotificacionesEjecucionRoleArn);

			// Creación de role para la función lambda...
			IRole roleLambda = new Role(this, $"{appName}APILambdaRole", new RoleProps {
                RoleName = $"{appName}APILambdaRole",
                Description = $"Role para API Lambda de {appName}",
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                ManagedPolicies = [
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaVPCAccessExecutionRole"),
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole"),
                ],
                InlinePolicies = new Dictionary<string, PolicyDocument> {
                    {
                        $"{appName}APILambdaPolicy",
                        new PolicyDocument(new PolicyDocumentProps {
                            Statements = [
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToSecretManager",
                                    Actions = [
                                        "secretsmanager:GetSecretValue"
                                    ],
                                    Resources = [
                                        secretArnConnectionString,
										secret.SecretArn,
									],
                                }),
								new PolicyStatement(new PolicyStatementProps{
									Sid = $"{appName}AccessToApiKey",
									Actions = [
										"apigateway:GET"
									],
									Resources = [
										$"arn:aws:apigateway:{this.Region}::/apikeys/{parameterHermesApiKeyId.StringValue}",
										$"arn:aws:apigateway:{this.Region}::/apikeys/{parameterKairosApiKeyId.StringValue}",
									],
								}),
								new PolicyStatement(new PolicyStatementProps{
									Sid = $"{appName}AccessToCognito",
									Actions = [
										"cognito-idp:AdminGetUser",
                                        "cognito-idp:ConfirmSignUp",
										"cognito-idp:ResendConfirmationCode"
                                    ],
									Resources = [
										$"arn:aws:cognito-idp:{this.Region}:{this.Account}:userpool/{userPool.UserPoolId}",
									],
								}),
								new PolicyStatement(new PolicyStatementProps{
									Sid = $"{appName}AccessToS3",
									Actions = [
										"s3:GetObject",
										"s3:PutObject",
										"s3:PutObjectTagging",
										"s3:GetObjectTagging",
									],
									Resources = [
										$"{bucket.BucketArn}/*",
									],
								}),
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToDynamoDB",
                                    Actions = [
                                        "dynamodb:PutItem",
                                        "dynamodb:Query"
                                    ],
                                    Resources = [
                                        tablaRateLimits.TableArn,
                                        $"{tablaRateLimits.TableArn}/*",
                                    ],
                                }),
								new PolicyStatement(new PolicyStatementProps{
									Sid = $"{appName}AccessToKMSKey",
									Actions = [
										"kms:Decrypt",
									],
									Resources = [
										kmsKey.KeyArn,
									],
								}),
							]
                        })
                    }
                }
            });

			string[] subsToSkip = [
                notificacionesUserPoolClient.UserPoolClientId,
				cognitoTriggerUserPoolClient.UserPoolClientId
			];

			// Creación de la función lambda...
			Function function = new(this, $"{appName}APILambdaFunction", new FunctionProps {
                Runtime = Runtime.DOTNET_10,
                Handler = handler,
                Code = Code.FromAsset(publishZip),
                FunctionName = $"{appName}API",
                Timeout = Duration.Seconds(double.Parse(timeout)),
                MemorySize = double.Parse(memorySize),
                Architecture = Architecture.X86_64,
                LogGroup = logGroup,
                Environment = new Dictionary<string, string> {
                    { CONST_APP_NAME, appName },
                    { CONST_SECRET_ARN, secretArnConnectionString },
					{ "COGNITO_REGION", regionAws },
					{ "COGNITO_BASE_URL", userPoolDomain.BaseUrl() },
					{ "COGNITO_USER_POOL_ID", userPool.UserPoolId },
					{ "COGNITO_USER_POOL_CLIENT_ID", userPoolClient.UserPoolClientId },
					{ "COGNITO_CALLBACK_URLS", string.Join(',', callbackUrls) },
					{ "COGNITO_REFRESH_TOKEN_VALIDITY_MINUTES", refreshTokenValidityMinutes },
					{ "HERMES_API_URL", parameterHermesApiUrl.StringValue },
					{ "HERMES_API_KEY_ID", parameterHermesApiKeyId.StringValue },
					{ "HERMES_DE_NOMBRE", hermesDeNombre },
					{ "HERMES_DE_CORREO", hermesDeCorreo },
					{ "HERMES_DE_WHATSAPP", hermesDeWhatsapp },
					{ "KAIROS_API_URL", parameterKairosApiUrl.StringValue },
					{ "KAIROS_API_KEY_ID", parameterKairosApiKeyId.StringValue },
					{ "NOTIFICACIONES_LAMBDA_ARN", parameterNotificacionesLambdaArn.StringValue },
					{ "NOTIFICACIONES_EJECUCION_ROLE_ARN", parameterNotificacionesEjecucionRoleArn.StringValue },
					{ "BUCKET_NAME_DOCUMENTOS_ADJUNTOS", bucket.BucketName },
					{ "SECRET_ARN_APP", secret.SecretArn },
					{ "GOOGLE_OAUTH2_API_URL", googleOAuth2ApiUrl },
					{ "GOOGLE_OAUTH2_SCOPE", googleOAuth2Scope },
					{ "GOOGLE_OAUTH2_GRANT_TYPE", googleOAuth2GrantType },
					{ "GOOGLE_RECAPTCHA_API_URL", googleRecaptchaApiUrl },
					{ "GOOGLE_RECAPTCHA_PROJECT_ID", googleRecaptchaProjectId },
					{ "GOOGLE_RECAPTCHA_SITE_KEY", googleRecaptchaSiteKey },
					{ "DESTINATARIOS_NUEVO_MENSAJE", destinatariosNuevoMensaje },
					{ "FLOW_API_URL", flowApiUrl },
					{ "FLOW_URL_CALLBACK", flowUrlCallback },
					{ "FLOW_URL_RETORNO", flowUrlRetorno },
					{ "DYNAMODB_TABLE_NAME_RATE_LIMITS", tablaRateLimits.TableName },
					{ "RATE_LIMITS_SUBS_TO_SKIP", string.Join(',', subsToSkip) },
					{ "KMS_KEY_ARN", kmsKey.KeyArn },
					{ "URL_CODIGO_VERIFICACION", urlCodigoVerificacion }
                },
                Vpc = vpc,
                VpcSubnets = new SubnetSelection {
                    Subnets = [subnet1, subnet2]
                },
                SecurityGroups = [securityGroup],
                Role = roleLambda,
            });

            // Creación de access logs...
            LogGroup logGroupAccessLogs = new(this, $"{appName}APILogGroupAccessLogs", new LogGroupProps {
                LogGroupName = $"/aws/lambda/{appName}API/access_logs",
                Retention = RetentionDays.ONE_MONTH,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

			// Creación del API Gateway HTTP API con integración a la lambda...
			HttpApi lambdaHttpApi = new(this, $"{appName}APILambdaHttpApi", new HttpApiProps {
				ApiName = $"{appName}API",
				Description = $"HTTP API de {appName}",
				CorsPreflight = new CorsPreflightOptions {
					AllowOrigins = allowedDomains.Split(","),
					AllowMethods = [
						CorsHttpMethod.GET,
						CorsHttpMethod.POST,
						CorsHttpMethod.PUT,
						CorsHttpMethod.DELETE
					],
					AllowHeaders = ["Content-Type", "Authorization", "X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset", "Retry-After"],
					AllowCredentials = true,
					MaxAge = Duration.Days(10),
				},
				DisableExecuteApiEndpoint = true,
				CreateDefaultStage = false,
				IpAddressType = IpAddressType.DUAL_STACK
			});

			lambdaHttpApi.AddRoutes(new AddRoutesOptions {
				Path = "/public/{proxy+}",
				Methods = [
					HttpMethod.GET,
					HttpMethod.POST,
					HttpMethod.PUT,
					HttpMethod.DELETE
				],
				Integration = new HttpLambdaIntegration($"{appName}APIHttpLambdaIntegration", function),
				Authorizer = new HttpNoneAuthorizer(),
			});

			lambdaHttpApi.AddRoutes(new AddRoutesOptions {
				Path = "/{proxy+}",
				Methods = [
					HttpMethod.GET,
					HttpMethod.POST,
					HttpMethod.PUT,
					HttpMethod.DELETE
				],
				Integration = new HttpLambdaIntegration($"{appName}APIHttpLambdaIntegration", function),
				Authorizer = new HttpJwtAuthorizer(
					$"{appName}APIHttpJwtAuthorizer",
					$"https://cognito-idp.{regionAws}.amazonaws.com/{userPool.UserPoolId}",
					new HttpJwtAuthorizerProps {
						JwtAudience = [
							userPoolClient.UserPoolClientId,
							notificacionesUserPoolClient.UserPoolClientId,
							cognitoTriggerUserPoolClient.UserPoolClientId,
						]
					}
				),
			});

			CfnStage stage = new(this, $"{appName}APIStage", new CfnStageProps { 
				ApiId = lambdaHttpApi.ApiId,
				StageName = "prod",
				Description = $"Stage para produccion de la aplicacion {appName}",
				AutoDeploy = true,
				AccessLogSettings = new CfnStage.AccessLogSettingsProperty {
					DestinationArn = logGroupAccessLogs.LogGroupArn,
					Format = "{\"requestTime\":\"$context.requestTime\",\"requestId\":\"$context.requestId\",\"httpMethod\":\"$context.httpMethod\",\"path\":\"$context.path\",\"routeKey\":\"$context.routeKey\",\"status\":$context.status,\"responseLatency\":$context.responseLatency,\"integrationRequestId\":\"$context.integration.requestId\",\"functionResponseStatus\":\"$context.integration.status\",\"integrationLatency\":\"$context.integration.latency\",\"integrationServiceStatus\":\"$context.integration.integrationStatus\",\"authorizeResultStatus\":\"$context.authorizer.status\",\"authorizerRequestId\":\"$context.authorizer.requestId\",\"ip\":\"$context.identity.sourceIp\",\"userAgent\":\"$context.identity.userAgent\",\"principalId\":\"$context.authorizer.principalId\"}"
				},
			});

            // Creación de la CfnApiMapping para el API Gateway...
			CfnApiMapping apiMapping = new(this, $"{appName}APIApiMapping", new CfnApiMappingProps {
                DomainName = domainName,
                ApiId = lambdaHttpApi.ApiId,
				Stage = stage.StageName,
            });
			apiMapping.Node.AddDependency(stage);
			apiMapping.Node.AddDependency(apiGatewayDomain);

			// Se configura permisos para la ejecucíon de la Lambda desde el API Gateway...
			ArnPrincipal arnPrincipal = new("apigateway.amazonaws.com");
            Permission permission = new() {
                Scope = this,
                Action = "lambda:InvokeFunction",
                Principal = arnPrincipal,
                SourceArn = $"arn:aws:execute-api:{this.Region}:{this.Account}:{lambdaHttpApi.ApiId}/*/*/*",
            };
            function.AddPermission($"{appName}APIPermission", permission);

            _ = new StringParameter(this, $"{appName}StringParameterApiUrl", new StringParameterProps {
                ParameterName = $"/{appName}/Api/Url",
                Description = $"API URL de la aplicacion {appName}",
                StringValue = $"https://{apiMapping.DomainName}/{(!string.IsNullOrWhiteSpace(apiMapping.ApiMappingKey) ? $"{apiMapping.ApiMappingKey}/" : "")}",
                Tier = ParameterTier.STANDARD,
            });
            #endregion

            #region Initial Creation Lambda
            // Se crea función lambda que ejecute scripts para la creación del esquema, usuario de aplicación y migración de EFCore...
            // Primero creación de log group lambda de creación inicial...
            LogGroup logGroupInitialLambda = new(this, $"{appName}APIInitialCreationLambdaLogGroup", new LogGroupProps {
                LogGroupName = $"/aws/lambda/{appName}APIInitialCreationLambda/logs",
                Retention = RetentionDays.ONE_MONTH,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // Luego la creación del rol para la función lambda...
            IRole roleInitialLambda = new Role(this, $"{appName}APIInitialCreationLambdaRole", new RoleProps {
                RoleName = $"{appName}APIInitialCreationLambdaRole",
                Description = $"Role para Lambda de creacion inicial {appName}",
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                ManagedPolicies = [
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaVPCAccessExecutionRole"),
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole"),
                ],
                InlinePolicies = new Dictionary<string, PolicyDocument> {
                    {
                        $"{appName}APIInitialCreationLambdaPolicy",
                        new PolicyDocument(new PolicyDocumentProps {
                            Statements = [
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToSecretManager",
                                    Actions = [
                                        "secretsmanager:GetSecretValue"
                                    ],
                                    Resources = [
                                        secretArnConnectionString,
                                    ],
                                })
                            ]
                        })
                    }
                }
            });

            // Y el security group...
            SecurityGroup securityGroupInitialLambda = new(this, $"{appName}APIInitialCreationLambdaSecurityGroup", new SecurityGroupProps {
                Vpc = vpc,
                SecurityGroupName = $"{appName}APIInitialCreationLambda",
                Description = $"Security Group para Lambda de creacion inicial {appName}",
                AllowAllOutbound = true
            });
            rdsSecurityGroup.AddIngressRule(Peer.SecurityGroupId(securityGroupInitialLambda.SecurityGroupId), Port.POSTGRES, $"Ingress para funcion lambda de creacion inicial {appName}");

            // Creación de la función lambda
            Function functionInitial = new(this, $"{appName}APIInitialCreationLambda", new FunctionProps {
                Runtime = Runtime.DOTNET_10,
                Handler = initialCreationHandler,
                Code = Code.FromAsset(initialCreationPublishZip),
                FunctionName = $"{appName}APIInitialCreation",
                Timeout = Duration.Seconds(2 * 60),
                MemorySize = 256,
                Architecture = Architecture.ARM_64,
                LogGroup = logGroupInitialLambda,
                Environment = new Dictionary<string, string> {
					{ CONST_APP_NAME, appName },
					{ CONST_SECRET_ARN, secretArnConnectionString },
                    { "APP_SCHEMA_NAME", appSchemaName },
                    { "MIGRATION_SCRIPT", migrationScript }
                },
                Vpc = vpc,
                VpcSubnets = new SubnetSelection {
                    Subnets = [subnet1, subnet2]
                },
                SecurityGroups = [securityGroupInitialLambda],
                Role = roleInitialLambda,
            });

            // Se gatilla la lambda...
            _ = new AwsCustomResource(this, $"{appName}APIInitialCreationTrigger", new AwsCustomResourceProps {
                Policy = AwsCustomResourcePolicy.FromStatements([
                    new PolicyStatement(new PolicyStatementProps{
                        Actions = [ "lambda:InvokeFunction" ],
                        Resources = [functionInitial.FunctionArn ]
                    })
                ]),
                Timeout = Duration.Seconds(2 * 60),
                OnUpdate = new AwsSdkCall {
                    Service = "Lambda",
                    Action = "invoke",
                    Parameters = new Dictionary<string, object> {
                        { "FunctionName", functionInitial.FunctionName },
                        { "InvocationType", "Event" },
                        { "Payload", "\"\"" }
                    },
                    PhysicalResourceId = PhysicalResourceId.Of(DateTime.Now.ToString())
                }
            });
            #endregion
        }
    }
}
