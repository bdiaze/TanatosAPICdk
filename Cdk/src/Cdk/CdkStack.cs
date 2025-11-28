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
using Amazon.CDK.AWS.SSM;
using Amazon.CDK.AwsApigatewayv2Authorizers;
using Amazon.CDK.AwsApigatewayv2Integrations;
using Amazon.CDK.CustomResources;
using Amazon.JSII.JsonModel.FileSystem;
using Constructs;
using System;
using System.Collections.Generic;
using System.IO;
using CfnStage = Amazon.CDK.AWS.Apigatewayv2.CfnStage;
using CfnStageProps = Amazon.CDK.AWS.Apigatewayv2.CfnStageProps;
using DomainNameAttributes = Amazon.CDK.AWS.Apigatewayv2.DomainNameAttributes;
using HttpMethod = Amazon.CDK.AWS.Apigatewayv2.HttpMethod;
using IDomainName = Amazon.CDK.AWS.Apigatewayv2.IDomainName;
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

			// Para API Authorizer...
			string apiAuthorizerPublishZip = System.Environment.GetEnvironmentVariable("API_AUTHORIZER_PUBLISH_ZIP") ?? throw new ArgumentNullException("API_AUTHORIZER_PUBLISH_ZIP");

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

			UserPoolDomain domain = new(this, $"{appName}CognitoDomain", new UserPoolDomainProps {
				UserPool = userPool,
				CustomDomain = new CustomDomainOptions {
					DomainName = cognitoCustomDomain,
					Certificate = certificate,
				},
				ManagedLoginVersion = ManagedLoginVersion.NEWER_MANAGED_LOGIN,
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
					Scopes = [OAuthScope.OPENID, OAuthScope.EMAIL, OAuthScope.PROFILE]
				}
			});

			// string base64Favicon = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recursos", "FAVICON.ico")));
			// string base64FormLogo = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recursos", "FORM_LOGO.png")));

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
							{ "colorSchemeMode", "LIGHT" }
						}}
					}},
					{ "componentClasses", new Dictionary<string, object>{
						{ "focusState", new Dictionary<string, object>{
							{ "lightMode", new Dictionary<string, object> {
								{ "borderColor", "0069d9ff" }
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
									{ "textColor", "1b6ec2ff" }
								}},
								{ "hover", new Dictionary<string, object>{
									{ "textColor", "0069d9ff" }
								}},
							}}
						}}
					}},
					{ "components", new Dictionary<string, object>{
						{ "favicon", new Dictionary<string, object> {
							{ "enabledTypes", new string[1] { "ICO" }},
						}},
						{ "form", new Dictionary<string, object> {
							{ "logo", new Dictionary<string, object> {
								// { "enabled", true }
							}},
						}},
						{ "pageBackground", new Dictionary<string, object> {
							{ "image", new Dictionary<string, object> {
								{ "enabled", false }
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
									{ "backgroundColor", "1b6ec2ff" },
									{ "textColor", "ffffffff" }
								}},
								{ "hover", new Dictionary<string, object>{
									{ "backgroundColor", "0069d9ff" },
									{ "textColor", "ffffffff" }
								}},
							}},
						}},
						{ "secondaryButton", new Dictionary<string, object> {
							{ "lightMode", new Dictionary<string, object> {
								{ "defaults", new Dictionary<string, object>{
									{ "backgroundColor", "ffffffff" },
									{ "borderColor", "1b6ec2ff" },
									{ "textColor", "1b6ec2ff" }
								}},
								{ "hover", new Dictionary<string, object>{
									{ "backgroundColor", "f2f8fdff" },
									{ "borderColor", "0069d9ff" },
									{ "textColor", "0069d9ff" }
								}},
							}},
						}}
					}}
				},
                /*
				Assets = (new List<CfnManagedLoginBranding.AssetTypeProperty>() {
					new() {
						Category = "FORM_LOGO",
						ColorMode = "LIGHT",
						Extension = "PNG",
						Bytes = base64FormLogo,
					},
					new() {
						Category = "FAVICON_ICO",
						ColorMode = "LIGHT",
						Extension = "ICO",
						Bytes = base64Favicon,
					}
				}).ToArray()
                */
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
			#endregion

			#region API Authorizer
			// Creación de log group lambda...
			LogGroup authorizerLogGroup = new(this, $"{appName}APIAuthorizerLogGroup", new LogGroupProps {
				LogGroupName = $"/aws/lambda/{appName}APIAuthorizer/logs",
				Retention = RetentionDays.ONE_MONTH,
				RemovalPolicy = RemovalPolicy.DESTROY
			});

			// Creación de role para la función lambda...
			IRole authorizerRoleLambda = new Role(this, $"{appName}APIAuthorizerRole", new RoleProps {
				RoleName = $"{appName}APIAuthorizerRole",
				Description = $"Role para Lambda Authorizer de API {appName}",
				AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
				ManagedPolicies = [
					ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaVPCAccessExecutionRole"),
					ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole"),
				],
			});

			Function authorizerFunction = new(this, $"{appName}APIAuthorizerFunction", new FunctionProps {
				FunctionName = $"{appName}APIAuthorizer",
				Description = $"API encargada de autorizar el acceso a la API de la aplicacion {appName}",
				Runtime = Runtime.PROVIDED_AL2023,
				Code = Code.FromAsset($"{apiAuthorizerPublishZip}"),
				Handler = "bootstrap",
				Timeout = Duration.Seconds(5),
				MemorySize = 128,
				Architecture = Architecture.X86_64,
				LogGroup = authorizerLogGroup,
				Environment = new Dictionary<string, string> {
					{ "APP_NAME", appName },
					{ "COGNITO_REGION", regionAws },
					{ "COGNITO_USER_POOL_ID", userPool.UserPoolId },
				},
				Role = authorizerRoleLambda,
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
                                })
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
					AllowMethods = [CorsHttpMethod.ANY],
					AllowHeaders = ["*"],
					MaxAge = Duration.Days(10),
				},
				DisableExecuteApiEndpoint = true,
				CreateDefaultStage = false,
			});

			lambdaHttpApi.AddRoutes(new AddRoutesOptions {
				Path = "/{proxy+}",
				Methods = [HttpMethod.ANY],
				Integration = new HttpLambdaIntegration($"{appName}APIHttpLambdaIntegration", function),
				Authorizer = new HttpLambdaAuthorizer(
					$"{appName}APILambdaAuthorizer",
					authorizerFunction,
					new HttpLambdaAuthorizerProps {
						ResponseTypes = [ HttpLambdaResponseType.SIMPLE ],
						IdentitySource = [],
						ResultsCacheTtl = Duration.Seconds(0),
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
					Format = "'{\"requestTime\":\"$context.requestTime\",\"requestId\":\"$context.requestId\",\"httpMethod\":\"$context.httpMethod\",\"path\":\"$context.path\",\"resourcePath\":\"$context.resourcePath\",\"status\":$context.status,\"responseLatency\":$context.responseLatency,\"xrayTraceId\":\"\",\"integrationRequestId\":\"$context.integration.requestId\",\"functionResponseStatus\":\"$context.integration.status\",\"integrationLatency\":\"$context.integration.latency\",\"integrationServiceStatus\":\"$context.integration.integrationStatus\",\"authorizeStatus\":\"\",\"authorizerStatus\":\"$context.authorizer.status\",\"authorizerLatency\":\"$context.authorizer.latency\",\"authorizerRequestId\":\"$context.authorizer.requestId\",\"ip\":\"$context.identity.sourceIp\",\"userAgent\":\"$context.identity.userAgent\",\"principalId\":\"$context.authorizer.principalId\"}'"
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
