using System.Text.Json;

namespace TanatosAPI.Helpers {
    public class ConnectionStringHelper(IHostEnvironment env, IConfiguration config, VariableEntornoHelper variableEntorno, SecretManagerHelper secretManager) {

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

                connectionString =
                    $"Server={secretConnectionString["Host"]};" +
                    $"Port={secretConnectionString["Port"]};" +
                    $"Database={secretConnectionString[$"{appName}Database"]};" +
                    $"User Id={secretConnectionString[$"{appName}AppUsername"]};" +
                    $"Password='{secretConnectionString[$"{appName}AppPassword"]}';";

                if (env.IsProduction()) {
                    connectionString += "Ssl Mode=Require;";
                    connectionString += "Trust Server Certificate=true;";
                }
            }

            return connectionString;
        }
    }
}
