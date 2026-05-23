using Npgsql;

namespace TanatosAPI.Helpers {
    public class DatabaseConnectionHelper(NpgsqlDataSource dataSource) {

        public async Task<NpgsqlConnection> ObtenerConexion() {
            return await dataSource.OpenConnectionAsync(); ;
        }
    }
}
