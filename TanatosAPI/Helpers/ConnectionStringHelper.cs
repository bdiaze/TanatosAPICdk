using Npgsql;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Helpers {
    public class ConnectionStringHelper(IHostEnvironment env, IConfiguration config, IFileHelper fileHelper, IVariableEntornoHelper variableEntorno, ISecretManagerHelper secretManager) : IConnectionStringHelper {

        private string? connectionString = null;

        public async Task<string> Obtener() {
            if (connectionString == null) {
                string appName = variableEntorno.Obtener("APP_NAME");

                Dictionary<string, string> secretConnectionString;
                if (env.IsProduction()) {
                    secretConnectionString = JsonSerializer.Deserialize(
                        await secretManager.ObtenerSecreto(variableEntorno.Obtener("SECRET_ARN_CONNECTION_STRING")),
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

				NpgsqlConnectionStringBuilder builder = new()  { 
                    Host = secretConnectionString["Host"],
                    Port = int.Parse(secretConnectionString["Port"]),
                    Database = secretConnectionString[$"{appName}Database"],
                    Username = secretConnectionString[$"{appName}AppUsername"],
                    Password = secretConnectionString[$"{appName}AppPassword"],
                };

                if (env.IsProduction()) {
                    string pathCert = Path.Combine(AppContext.BaseDirectory, "Certs", "global-bundle.pem");
					// Se valida que existe el root certificate...
                    if (!fileHelper.Exists(pathCert)) throw new FileNotFoundException("No se encontró el certificado raíz para la conexión SSL", pathCert);

					builder.SslMode = SslMode.VerifyFull;
                    builder.RootCertificate = pathCert;
                }

                connectionString = builder.ToString();
			}

            return connectionString;
        }
    }
}
