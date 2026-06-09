using Npgsql;

namespace TanatosAPI.Interfaces {
	public interface IDatabaseConnectionHelper {
		public Task<NpgsqlConnection> ObtenerConexion();

		public Task<IDatabaseConnection> ObtenerConexionWrapper();
	}
}
