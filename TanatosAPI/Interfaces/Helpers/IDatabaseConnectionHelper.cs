using Npgsql;

namespace TanatosAPI.Interfaces.Helpers {
	public interface IDatabaseConnectionHelper {
		public Task<NpgsqlConnection> ObtenerConexion();

		public Task<IDatabaseConnection> ObtenerConexionWrapper();
	}
}
