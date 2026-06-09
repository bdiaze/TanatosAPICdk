using Npgsql;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
    [ExcludeFromCodeCoverage]
    public class DatabaseConnectionHelper(NpgsqlDataSource dataSource) : IDatabaseConnectionHelper {

        public async Task<NpgsqlConnection> ObtenerConexion() {
            return await dataSource.OpenConnectionAsync();
        }

		public async Task<IDatabaseConnection> ObtenerConexionWrapper() {
            return new DatabaseConnectionWrapper(await dataSource.OpenConnectionAsync());
		}
	}
}
