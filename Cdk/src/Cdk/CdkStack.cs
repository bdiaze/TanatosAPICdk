using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.CustomResources;
using Constructs;
using System;
using System.Collections.Generic;
using StageOptions = Amazon.CDK.AWS.APIGateway.StageOptions;

namespace Cdk
{
    public class CdkStack : Stack
    {
        internal CdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            string appName = System.Environment.GetEnvironmentVariable("APP_NAME") ?? throw new ArgumentNullException("APP_NAME");
            
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

            // Creación de la LambdaRestApi...
            LambdaRestApi lambdaRestApi = new(this, $"{appName}APILambdaRestApi", new LambdaRestApiProps {
                Handler = function,
                DefaultCorsPreflightOptions = new CorsOptions {
                    AllowOrigins = allowedDomains.Split(","),
                },
                DeployOptions = new StageOptions {
                    AccessLogDestination = new LogGroupLogDestination(logGroupAccessLogs),
                    AccessLogFormat = AccessLogFormat.Custom("'{\"requestTime\":\"$context.requestTime\",\"requestId\":\"$context.requestId\",\"httpMethod\":\"$context.httpMethod\",\"path\":\"$context.path\",\"resourcePath\":\"$context.resourcePath\",\"status\":$context.status,\"responseLatency\":$context.responseLatency,\"xrayTraceId\":\"$context.xrayTraceId\",\"integrationRequestId\":\"$context.integration.requestId\",\"functionResponseStatus\":\"$context.integration.status\",\"integrationLatency\":\"$context.integration.latency\",\"integrationServiceStatus\":\"$context.integration.integrationStatus\",\"authorizeStatus\":\"$context.authorize.status\",\"authorizerStatus\":\"$context.authorizer.status\",\"authorizerLatency\":\"$context.authorizer.latency\",\"authorizerRequestId\":\"$context.authorizer.requestId\",\"ip\":\"$context.identity.sourceIp\",\"userAgent\":\"$context.identity.userAgent\",\"principalId\":\"$context.authorizer.principalId\"}'"),
                    StageName = "prod",
                    Description = $"Stage para produccion de la aplicacion {appName}",
                },
                RestApiName = $"{appName}API",
                DefaultMethodOptions = new MethodOptions {
                    ApiKeyRequired = true,
                },
            });

            // Creación de la CfnApiMapping para el API Gateway...
            CfnApiMapping apiMapping = new(this, $"{appName}APIApiMapping", new CfnApiMappingProps {
                DomainName = domainName,
                ApiMappingKey = apiMappingKey,
                ApiId = lambdaRestApi.RestApiId,
                Stage = lambdaRestApi.DeploymentStage.StageName,
            });

            // Se crea Usage Plan para configurar API Key...
            UsagePlan usagePlan = new(this, $"{appName}APIUsagePlan", new UsagePlanProps {
                Name = $"{appName}APIUsagePlan",
                Description = $"Usage Plan de {appName} API",
                ApiStages = [
                    new UsagePlanPerApiStage() {
                        Api = lambdaRestApi,
                        Stage = lambdaRestApi.DeploymentStage
                    }
                ],
            });

            // Se crea API Key...
            ApiKey apiGatewayKey = new(this, $"{appName}APIKey", new ApiKeyProps {
                ApiKeyName = $"{appName}APIKey",
                Description = $"API Key de {appName}",
            });
            usagePlan.AddApiKey(apiGatewayKey);

            // Se configura permisos para la ejecucíon de la Lambda desde el API Gateway...
            ArnPrincipal arnPrincipal = new("apigateway.amazonaws.com");
            Permission permission = new() {
                Scope = this,
                Action = "lambda:InvokeFunction",
                Principal = arnPrincipal,
                SourceArn = $"arn:aws:execute-api:{this.Region}:{this.Account}:{lambdaRestApi.RestApiId}/*/*/*",
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
