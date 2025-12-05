using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TipoFiscalizadorDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<TipoFiscalizador?> ObtenerPorId(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.QueryFirstOrDefaultAsync<TipoFiscalizador>(
				"SELECT ID, NOMBRE, NOMBRE_CORTO, VIGENCIA FROM TANATOS.TIPO_FISCALIZADOR WHERE ID = @ID",
				new { id }
			);
		}

		public async Task<List<TipoFiscalizador>> ObtenerPorVigencia(bool vigencia) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TipoFiscalizador>(
				"SELECT ID, NOMBRE, NOMBRE_CORTO, VIGENCIA FROM TANATOS.TIPO_FISCALIZADOR WHERE VIGENCIA = @VIGENCIA",
				new { vigencia }
			)];
		}

		public async Task Insertar(TipoFiscalizador item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"INSERT INTO TANATOS.TIPO_FISCALIZADOR(ID, NOMBRE, NOMBRE_CORTO, VIGENCIA) VALUES (@ID, @NOMBRE, @NOMBRECORTO, @VIGENCIA)",
				new { item.Id, item.Nombre, item.NombreCorto, item.Vigencia }
			);
		}

		public async Task Actualizar(TipoFiscalizador item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.TIPO_FISCALIZADOR SET NOMBRE = @NOMBRE, NOMBRE_CORTO = @NOMBRECORTO, VIGENCIA = @VIGENCIA WHERE ID = @ID",
				new { item.Nombre, item.NombreCorto, item.Vigencia, item.Id }
			);
		}

		public async Task Eliminar(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"DELETE FROM TANATOS.TIPO_FISCALIZADOR WHERE ID = @ID",
				new { id }
			);
		}
	}
}
