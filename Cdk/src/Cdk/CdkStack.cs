using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.Batch;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.Route53.Targets;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SecretsManager;
using Amazon.CDK.AWS.SSM;
using Amazon.CDK.AwsApigatewayv2Authorizers;
using Amazon.CDK.AwsApigatewayv2Integrations;
using Amazon.CDK.CustomResources;
using Constructs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CfnStage = Amazon.CDK.AWS.Apigatewayv2.CfnStage;
using CfnStageProps = Amazon.CDK.AWS.Apigatewayv2.CfnStageProps;
using DomainNameAttributes = Amazon.CDK.AWS.Apigatewayv2.DomainNameAttributes;
using HttpMethod = Amazon.CDK.AWS.Apigatewayv2.HttpMethod;
using IDomainName = Amazon.CDK.AWS.Apigatewayv2.IDomainName;
using Secret = Amazon.CDK.AWS.SecretsManager.Secret;
using StageOptions = Amazon.CDK.AWS.APIGateway.StageOptions;

namespace Cdk
{
    public class CdkStack : Stack
    {
        internal CdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            string appName = System.Environment.GetEnvironmentVariable("APP_NAME") ?? throw new ArgumentNullException("APP_NAME");
			string regionAws = System.Environment.GetEnvironmentVariable("REGION_AWS") ?? throw new ArgumentNullException("REGION_AWS");

			// Para cognito...
			string emailSubject = System.Environment.GetEnvironmentVariable("VERIFICATION_SUBJECT") ?? throw new ArgumentNullException("VERIFICATION_SUBJECT");
			string emailBody = System.Environment.GetEnvironmentVariable("VERIFICATION_BODY") ?? throw new ArgumentNullException("VERIFICATION_BODY");

			string cognitoDomainName = System.Environment.GetEnvironmentVariable("COGNITO_DOMAIN_NAME") ?? throw new ArgumentNullException("COGNITO_DOMAIN_NAME");
			string cognitoCustomDomain = System.Environment.GetEnvironmentVariable("COGNITO_CUSTOM_DOMAIN") ?? throw new ArgumentNullException("COGNITO_CUSTOM_DOMAIN");
			string arnCognitoCertificate = System.Environment.GetEnvironmentVariable("ARN_COGNITO_CERTIFICATE") ?? throw new ArgumentNullException("ARN_COGNITO_CERTIFICATE");

			string[] callbackUrls = System.Environment.GetEnvironmentVariable("CALLBACK_URLS").Split(",") ?? throw new ArgumentNullException("CALLBACK_URLS");
			string[] logoutUrls = System.Environment.GetEnvironmentVariable("LOGOUT_URLS").Split(",") ?? throw new ArgumentNullException("LOGOUT_URLS");
			string accessTokenValidityMinutes = System.Environment.GetEnvironmentVariable("ACCESS_TOKEN_VALIDITY_MINUTES") ?? throw new ArgumentNullException("ACCESS_TOKEN_VALIDITY_MINUTES");
			string idTokenValidityMinutes = System.Environment.GetEnvironmentVariable("ID_TOKEN_VALIDITY_MINUTES") ?? throw new ArgumentNullException("ID_TOKEN_VALIDITY_MINUTES");
			string refreshTokenValidityMinutes = System.Environment.GetEnvironmentVariable("REFRESH_TOKEN_VALIDITY_MINUTES") ?? throw new ArgumentNullException("REFRESH_TOKEN_VALIDITY_MINUTES");

			// Para proceso de notificación...
			string notificacionesTokenValidityMinutes = System.Environment.GetEnvironmentVariable("NOTIFICACIONES_TOKEN_VALIDITY_MINUTES") ?? throw new ArgumentNullException("NOTIFICACIONES_TOKEN_VALIDITY_MINUTES");

			// Para infraestructura...
			string publishZip = System.Environment.GetEnvironmentVariable("PUBLISH_ZIP") ?? throw new ArgumentNullException("PUBLISH_ZIP");
            string handler = System.Environment.GetEnvironmentVariable("HANDLER") ?? throw new ArgumentNullException("HANDLER");
            string timeout = System.Environment.GetEnvironmentVariable("TIMEOUT") ?? throw new ArgumentNullException("TIMEOUT");
            string memorySize = System.Environment.GetEnvironmentVariable("MEMORY_SIZE") ?? throw new ArgumentNullException("MEMORY_SIZE");
            string domainName = System.Environment.GetEnvironmentVariable("DOMAIN_NAME") ?? throw new ArgumentNullException("DOMAIN_NAME");
            string apiMappingKey = System.Environment.GetEnvironmentVariable("API_MAPPING_KEY") ?? throw new ArgumentNullException("API_MAPPING_KEY");
            string vpcId = System.Environment.GetEnvironmentVariable("VPC_ID") ?? throw new ArgumentNullException("VPC_ID");
            string privateWithInternetId1 = System.Environment.GetEnvironmentVariable("PRIVATE_WITH_INTERNET_ID_1") ?? throw new ArgumentNullException("PRIVATE_WITH_INTERNET_ID_1");
            string privateWithInternetId2 = System.Environment.GetEnvironmentVariable("PRIVATE_WITH_INTERNET_ID_2") ?? throw new ArgumentNullException("PRIVATE_WITH_INTERNET_ID_2");
            string rdsSecurityGroupId = System.Environment.GetEnvironmentVariable("RDS_SECURITY_GROUP_ID") ?? throw new ArgumentNullException("RDS_SECURITY_GROUP_ID")!;

            // Variables de entorno de la lambda...
            string secretArnConnectionString = System.Environment.GetEnvironmentVariable("SECRET_ARN_CONNECTION_STRING") ?? throw new ArgumentNullException("SECRET_ARN_CONNECTION_STRING");
            string allowedDomains = System.Environment.GetEnvironmentVariable("ALLOWED_DOMAINS") ?? throw new ArgumentNullException("ALLOWED_DOMAINS");
			string arnParameterHermesApiUrl = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_HERMES_API_URL") ?? throw new ArgumentNullException("ARN_PARAMETER_HERMES_API_URL");
			string arnParameterHermesApiKeyId = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_HERMES_API_KEY_ID") ?? throw new ArgumentNullException("ARN_PARAMETER_HERMES_API_KEY_ID");
			string hermesDeNombre = System.Environment.GetEnvironmentVariable("HERMES_DE_NOMBRE") ?? throw new ArgumentNullException("HERMES_DE_NOMBRE");
			string hermesDeCorreo = System.Environment.GetEnvironmentVariable("HERMES_DE_CORREO") ?? throw new ArgumentNullException("HERMES_DE_CORREO");
			string arnParameterKairosApiUrl = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_KAIROS_API_URL") ?? throw new ArgumentNullException("ARN_PARAMETER_KAIROS_API_URL");
			string arnParameterKairosApiKeyId = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_KAIROS_API_KEY_ID") ?? throw new ArgumentNullException("ARN_PARAMETER_KAIROS_API_KEY_ID");
			string arnParameterNotificacionesLambdaArn = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_NOTIFICACIONES_LAMBDA_ARN") ?? throw new ArgumentNullException("ARN_PARAMETER_NOTIFICACIONES_LAMBDA_ARN");
			string arnParameterNotificacionesEjecucionRoleArn = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_NOTIFICACIONES_EJECUCION_ROLE_ARN") ?? throw new ArgumentNullException("ARN_PARAMETER_NOTIFICACIONES_EJECUCION_ROLE_ARN");

			// Variables de entorno para la lambda de ejecución inicial...
			string appSchemaName = System.Environment.GetEnvironmentVariable("APP_SCHEMA_NAME") ?? throw new ArgumentNullException("APP_SCHEMA_NAME");
            string initialCreationHandler = System.Environment.GetEnvironmentVariable("INITIAL_CREATION_HANDLER") ?? throw new ArgumentNullException("INITIAL_CREATION_HANDLER");
            string initialCreationPublishZip = System.Environment.GetEnvironmentVariable("INITIAL_CREATION_PUBLISH_ZIP") ?? throw new ArgumentNullException("INITIAL_CREATION_PUBLISH_ZIP");
            string migrationScript = System.Environment.GetEnvironmentVariable("MIGRATION_SCRIPT") ?? throw new ArgumentNullException("MIGRATION_SCRIPT");

            // Se obtiene la VPC y subnets...
            IVpc vpc = Vpc.FromLookup(this, $"{appName}Vpc", new VpcLookupOptions {
                VpcId = vpcId
            });

            ISubnet subnet1 = Subnet.FromSubnetId(this, $"{appName}Subnet1", privateWithInternetId1);
            ISubnet subnet2 = Subnet.FromSubnetId(this, $"{appName}Subnet2", privateWithInternetId2);

			// Se busca certificado de cognito creado anteriormente...
			ICertificate certificate = Certificate.FromCertificateArn(this, $"{appName}CognitoCertificate", arnCognitoCertificate);

			#region Cognito
			UserPool userPool = new(this, $"{appName}UserPool", new UserPoolProps {
				UserPoolName = $"{appName}UserPool",
				SelfSignUpEnabled = true,
				SignInCaseSensitive = false,
				UserVerification = new UserVerificationConfig {
					EmailSubject = emailSubject,
					EmailBody = emailBody,
					EmailStyle = VerificationEmailStyle.CODE,
				},
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
			});

			_ = new UserPoolGroup(this, $"{appName}AdminUserGroup", new UserPoolGroupProps {
				GroupName = "Admin",
				UserPool = userPool,
				Description = $"Administrador de la aplicacion {appName}",
			});

			UserPoolDomain domain = new(this, $"{appName}CognitoDomain2", new UserPoolDomainProps {
				UserPool = userPool,
				CustomDomain = new CustomDomainOptions {
					DomainName = cognitoCustomDomain,
					Certificate = certificate,
				},
				ManagedLoginVersion = ManagedLoginVersion.NEWER_MANAGED_LOGIN,
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
            #endregion


            UserPoolResourceServer resourceServer =  userPool.AddResourceServer($"{appName}ResourceServer", new UserPoolResourceServerOptions { 
				Identifier = "api",
				Scopes = [
					scopeObligacionesReadSelf,
					scopeObligacionesWriteSelf,
					scopeNegociosReadSelf,
                    scopeNegociosWriteSelf,
                    scopeVencimientosReadSelf,
                    scopeVencimientosWriteSelf,
                    scopeTemplatesReadPublic,
                    scopeSistemaReadPublic,
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
					UserPoolClientIdentityProvider.COGNITO
                ],
				OAuth = new OAuthSettings {
					CallbackUrls = callbackUrls,
					LogoutUrls = logoutUrls,
					Flows = new OAuthFlows { AuthorizationCodeGrant = true },
					Scopes = [
						OAuthScope.OPENID, OAuthScope.EMAIL, OAuthScope.PROFILE,
						OAuthScope.ResourceServer(resourceServer, scopeObligacionesReadSelf),
						OAuthScope.ResourceServer(resourceServer, scopeObligacionesWriteSelf),
						OAuthScope.ResourceServer(resourceServer, scopeNegociosReadSelf),
						OAuthScope.ResourceServer(resourceServer, scopeNegociosWriteSelf),
						OAuthScope.ResourceServer(resourceServer, scopeVencimientosReadSelf),
						OAuthScope.ResourceServer(resourceServer, scopeVencimientosWriteSelf),
						OAuthScope.ResourceServer(resourceServer, scopeTemplatesReadPublic),
						OAuthScope.ResourceServer(resourceServer, scopeSistemaReadPublic),
					]
				},
				AccessTokenValidity = Duration.Minutes(double.Parse(accessTokenValidityMinutes)),
				IdTokenValidity = Duration.Minutes(double.Parse(idTokenValidityMinutes)),
				RefreshTokenValidity = Duration.Minutes(double.Parse(refreshTokenValidityMinutes))
			});

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

			string base64Favicon = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recursos", "FAVICON.ico")));
			string base64FormLogo = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recursos", "FORM_LOGO.png")));
			string base64PageHeaderLogo = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recursos", "PAGE_HEADER_LOGO.png")));
			string base64PageFooterLogo = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recursos", "PAGE_FOOTER_LOGO.png")));
			string base64BackgroundImage = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recursos", "BACKGROUND_IMAGE.jpeg")));

			_ = new CfnManagedLoginBranding(this, $"{appName}ManagedLoginBranding", new CfnManagedLoginBrandingProps {
				UserPoolId = userPool.UserPoolId,
				ClientId = userPoolClient.UserPoolClientId,
				ReturnMergedResources = true,
				Settings = new Dictionary<string, object> {
					{ "categories", new Dictionary<string, object> {
						{ "form", new Dictionary<string, object> {
							{ "languageSelector", new Dictionary<string, object> {
								{ "enabled", true }
							}}
						}},
						{ "global", new Dictionary<string, object> {
							{ "colorSchemeMode", "LIGHT" },
							{ "pageHeader", new Dictionary<string, object> {
								{ "enabled", true }
							}},
							{ "pageFooter", new Dictionary<string, object> {
								{ "enabled", true }
							}}
						}}
					}},
					{ "componentClasses", new Dictionary<string, object>{
						{ "buttons", new Dictionary<string, object>{
							{ "borderRadius", 16.0 }
						}},
						{ "optionControls", new Dictionary<string, object>{
							{ "lightMode", new Dictionary<string, object> {
								{ "selected", new Dictionary<string, object>{
									{ "backgroundColor", "005db2ff" }
								}},
							}}
						}},
						{ "focusState", new Dictionary<string, object>{
							{ "lightMode", new Dictionary<string, object> {
								{ "borderColor", "02b2cbff" }
							}}
						}},
						{ "input", new Dictionary<string, object>{
							{ "lightMode", new Dictionary<string, object> {
								{ "defaults", new Dictionary<string, object>{
                                    // { "borderColor", "0069d9ff" }
                                }},
								{ "placeholderColor", "6c757dff" },
							}}
						}},
						{ "inputLabel", new Dictionary<string, object>{
							{ "lightMode", new Dictionary<string, object> {
                                // { "textColor", "6c757dff" }
                            }}
						}},
						{ "link", new Dictionary<string, object>{
							{ "lightMode", new Dictionary<string, object> {
								{ "defaults", new Dictionary<string, object>{
									{ "textColor", "005db2ff" }
								}},
								{ "hover", new Dictionary<string, object>{
									{ "textColor", "005db2cc" }
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
								{ "enabled", false }
							}},
							{ "logo", new Dictionary<string, object> {
								{ "enabled", true },
								{ "location", "START" }
							}},
							{ "lightMode", new Dictionary<string, object> {
								{ "background", new Dictionary<string, object>{
                                    { "color", "ffffffff" }
                                }},
								{ "borderColor", "f5f5f5ff" }
							}}
						}},
						{ "pageFooter", new Dictionary<string, object> {
							{ "backgroundImage", new Dictionary<string, object> {
								{ "enabled", false }
							}},
							{ "logo", new Dictionary<string, object> {
								{ "enabled", true },
								{ "location", "START" }
							}},
							{ "lightMode", new Dictionary<string, object> {
								{ "background", new Dictionary<string, object>{
									{ "color", "2a2d34ff" }
								}},
								{ "borderColor", "2a2d34ff" }
							}}
						}},
						{ "form", new Dictionary<string, object> {
							{ "borderRadius", 0.0 },
							{ "logo", new Dictionary<string, object> {
								{ "enabled", false }
							}},
						}},
						{ "pageBackground", new Dictionary<string, object> {
							{ "image", new Dictionary<string, object> {
								{ "enabled", true }
							}},
						}},
						{ "pageText", new Dictionary<string, object> {
							{ "lightMode", new Dictionary<string, object> {
								{ "headingColor", "212529ff" },
								{ "bodyColor", "212529ff" },
								{ "descriptionColor", "212529ff" },
							}},
						}},
						{ "primaryButton", new Dictionary<string, object> {
							{ "lightMode", new Dictionary<string, object> {
								{ "defaults", new Dictionary<string, object>{
									{ "backgroundColor", "02b2cbff" },
									{ "borderColor", "02b2cbff" },
									{ "textColor", "ffffffff" }
								}},
								{ "hover", new Dictionary<string, object>{
									{ "backgroundColor", "02b2cbcc" },
									{ "borderColor", "02b2cbcc" },
									{ "textColor", "ffffffff" }
								}},
								{ "active", new Dictionary<string, object>{
									{ "backgroundColor", "02b2cbff" },
									{ "borderColor", "02b2cbff" },
									{ "textColor", "ffffffff" }
								}},
							}},
						}},
						{ "secondaryButton", new Dictionary<string, object> {
							{ "lightMode", new Dictionary<string, object> {
								{ "defaults", new Dictionary<string, object>{
									{ "backgroundColor", "ffffffff" },
									{ "borderColor", "02b2cbff" },
									{ "textColor", "02b2cbff" }
								}},
								{ "hover", new Dictionary<string, object>{
									{ "backgroundColor", "ffffffff" },
									{ "borderColor", "02b2cbcc" },
									{ "textColor", "02b2cbff" }
								}},
								{ "active", new Dictionary<string, object>{
									{ "backgroundColor", "ffffffff" },
									{ "borderColor", "02b2cbff" },
									{ "textColor", "02b2cbff" }
								}},
							}},
						}}
					}}
				},
				Assets = (new List<CfnManagedLoginBranding.AssetTypeProperty>() {
					new() {
						Category = "FORM_LOGO",
						ColorMode = "LIGHT",
						Extension = "PNG",
						Bytes = base64FormLogo,
					},
					new() {
						Category = "PAGE_HEADER_LOGO",
						ColorMode = "LIGHT",
						Extension = "PNG",
						Bytes = base64PageHeaderLogo,
					},
					new() {
						Category = "PAGE_FOOTER_LOGO",
						ColorMode = "LIGHT",
						Extension = "PNG",
						Bytes = base64PageFooterLogo,
					},
					new() {
						Category = "PAGE_BACKGROUND",
						ColorMode = "LIGHT",
						Extension = "JPEG",
						Bytes = base64BackgroundImage,
					},
					new() {
						Category = "FAVICON_ICO",
						ColorMode = "LIGHT",
						Extension = "ICO",
						Bytes = base64Favicon,
					}
				}).ToArray()
			});

			IHostedZone hostedZone = HostedZone.FromLookup(this, $"{appName}HostedZone", new HostedZoneProviderProps {
				DomainName = cognitoDomainName
			});

			// Se crea record en hosted zone...
			_ = new ARecord(this, $"{appName}LoginARecord", new ARecordProps {
				Zone = hostedZone,
				RecordName = cognitoCustomDomain,
				Target = RecordTarget.FromAlias(new UserPoolDomainTarget(domain)),
			});


            // Se configuran parámetros para ser rescatados por consumidores...
            _ = new Secret(this, $"{appName}Secret", new SecretProps {
                SecretName = $"/{appName}",
                Description = $"Secretos de la aplicacion de {appName}",
                SecretObjectValue = new Dictionary<string, SecretValue> {
                    { "CognitoBaseUrl", SecretValue.UnsafePlainText(domain.BaseUrl()) },
                    { "NotificacionesUserPoolClientId", SecretValue.UnsafePlainText(notificacionesUserPoolClient.UserPoolClientId) },
                    { "NotificacionesUserPoolClientSecret", notificacionesUserPoolClient.UserPoolClientSecret },
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
										"cognito-idp:AdminGetUser"
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
										"s3:HeadObject",
										"s3:PutObjectTagging",
										"s3:GetObjectTagging",
									],
									Resources = [
										$"{bucket.BucketArn}/*",
									],
								}),
							]
                        })
                    }
                }
            });

			// Creación de la función lambda...
			Function function = new(this, $"{appName}APILambdaFunction", new FunctionProps {
                Runtime = Runtime.DOTNET_8,
                Handler = handler,
                Code = Code.FromAsset(publishZip),
                FunctionName = $"{appName}API",
                Timeout = Duration.Seconds(double.Parse(timeout)),
                MemorySize = double.Parse(memorySize),
                Architecture = Architecture.X86_64,
                LogGroup = logGroup,
                Environment = new Dictionary<string, string> {
                    { "APP_NAME", appName },
                    { "SECRET_ARN_CONNECTION_STRING", secretArnConnectionString },
					{ "COGNITO_REGION", regionAws },
					{ "COGNITO_BASE_URL", domain.BaseUrl() },
					{ "COGNITO_USER_POOL_ID", userPool.UserPoolId },
					{ "COGNITO_USER_POOL_CLIENT_ID", userPoolClient.UserPoolClientId },
					{ "COGNITO_NOTIFICACIONES_USER_POOL_CLIENT_ID", notificacionesUserPoolClient.UserPoolClientId },
					{ "COGNITO_CALLBACK_URLS", string.Join(',', callbackUrls) },
					{ "COGNITO_REFRESH_TOKEN_VALIDITY_MINUTES", refreshTokenValidityMinutes },
					{ "API_GATEWAY_MAPPING_KEY", apiMappingKey },
					{ "HERMES_API_URL", parameterHermesApiUrl.StringValue },
					{ "HERMES_API_KEY_ID", parameterHermesApiKeyId.StringValue },
					{ "HERMES_DE_NOMBRE", hermesDeNombre },
					{ "HERMES_DE_CORREO", hermesDeCorreo },
					{ "KAIROS_API_URL", parameterKairosApiUrl.StringValue },
					{ "KAIROS_API_KEY_ID", parameterKairosApiKeyId.StringValue },
					{ "NOTIFICACIONES_LAMBDA_ARN", parameterNotificacionesLambdaArn.StringValue },
					{ "NOTIFICACIONES_EJECUCION_ROLE_ARN", parameterNotificacionesEjecucionRoleArn.StringValue },
					{ "BUCKET_NAME_DOCUMENTOS_ADJUNTOS", bucket.BucketName }
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
					AllowHeaders = ["Content-Type", "X-CSRF-Token", "Authorization"],
					AllowCredentials = true,
					MaxAge = Duration.Days(10),
				},
				DisableExecuteApiEndpoint = true,
				CreateDefaultStage = false,
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
							notificacionesUserPoolClient.UserPoolClientId
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
                ApiMappingKey = apiMappingKey,
                ApiId = lambdaHttpApi.ApiId,
				Stage = stage.StageName,
            });
			apiMapping.Node.AddDependency(stage);

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
                StringValue = $"https://{apiMapping.DomainName}/{apiMapping.ApiMappingKey}/",
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
                Runtime = Runtime.DOTNET_8,
                Handler = initialCreationHandler,
                Code = Code.FromAsset(initialCreationPublishZip),
                FunctionName = $"{appName}APIInitialCreation",
                Timeout = Duration.Seconds(2 * 60),
                MemorySize = 256,
                Architecture = Architecture.ARM_64,
                LogGroup = logGroupInitialLambda,
                Environment = new Dictionary<string, string> {
                    { "SECRET_ARN_CONNECTION_STRING", secretArnConnectionString },
                    { "APP_NAME", appName },
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
