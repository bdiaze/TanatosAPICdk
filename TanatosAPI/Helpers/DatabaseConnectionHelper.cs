using Npgsql;

namespace TanatosAPI.Helpers {
    public class DatabaseConnectionHelper(ConnectionStringHelper connectionString) {

        private NpgsqlDataSource? dataSource = null;

        public async Task<NpgsqlConnection> ObtenerConexion() {
            if (dataSource == null) {
                string connString = await connectionString.Obtener();

				NpgsqlConnectionStringBuilder stringBuilder = new(connString) {
					MaxPoolSize = 5
				};

				NpgsqlDataSourceBuilder dataSourceBuilder = new(stringBuilder.ToString());
                dataSource = dataSourceBuilder.Build();
            }

            return await dataSource.OpenConnectionAsync();
        }
    }
}
