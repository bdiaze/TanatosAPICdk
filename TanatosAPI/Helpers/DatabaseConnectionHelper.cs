using Npgsql;
using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Helpers {
    [ExcludeFromCodeCoverage]
    public class DatabaseConnectionHelper(NpgsqlDataSource dataSource) {

        public async Task<NpgsqlConnection> ObtenerConexion() {
            return await dataSource.OpenConnectionAsync(); ;
        }
    }
}
