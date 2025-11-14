using Npgsql;

namespace TanatosAPI.Helpers {
    public class DatabaseConnectionHelper(ConnectionStringHelper connectionString) {

        private NpgsqlDataSource? dataSource = null;

        public async Task<NpgsqlConnection> ObtenerConexion() {
            if (dataSource == null) {
                string connString = await connectionString.Obtener();
                connString += "Maximum Pool Size=5;";

                NpgsqlDataSourceBuilder builder = new(connString);
                dataSource = builder.Build();
            }

            return await dataSource.OpenConnectionAsync();
        }
    }
}
