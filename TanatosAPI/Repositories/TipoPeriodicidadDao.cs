using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TipoPeriodicidadDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<TipoPeriodicidad?> ObtenerPorId(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.QueryFirstOrDefaultAsync<TipoPeriodicidad>(
				"SELECT ID, NOMBRE, DESCRIPCION, VIGENCIA FROM TANATOS.TIPO_PERIODICIDAD WHERE ID = @ID",
				new { id }
			);
		}

		public async Task<List<TipoPeriodicidad>> ObtenerPorVigencia(bool vigencia) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TipoPeriodicidad>(
				"SELECT ID, NOMBRE, DESCRIPCION, VIGENCIA FROM TANATOS.TIPO_PERIODICIDAD WHERE VIGENCIA = @VIGENCIA",
				new { vigencia }
			)];
		}

		public async Task Insertar(TipoPeriodicidad item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"INSERT INTO TANATOS.TIPO_PERIODICIDAD(ID, NOMBRE, DESCRIPCION, VIGENCIA) VALUES (@ID, @NOMBRE, @DESCRIPCION, @VIGENCIA)",
				new { item.Id, item.Nombre, item.Descripcion, item.Vigencia }
			);
		}

		public async Task Actualizar(TipoPeriodicidad item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.TIPO_PERIODICIDAD SET NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, VIGENCIA = @VIGENCIA WHERE ID = @ID",
				new { item.Nombre, item.Descripcion, item.Vigencia, item.Id }
			);
		}

		public async Task Eliminar(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"DELETE FROM TANATOS.TIPO_PERIODICIDAD WHERE ID = @ID",
				new { id }
			);
		}
	}
}
