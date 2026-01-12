using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TipoRubroDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<TipoRubro?> ObtenerPorId(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.QueryFirstOrDefaultAsync<TipoRubro>(
				"SELECT ID, NOMBRE, DESCRIPCION, VIGENCIA FROM TANATOS.TIPO_RUBRO WHERE ID = @ID",
				new { id }
			);
		}

		public async Task<List<TipoRubro>> ObtenerPorVigencia(bool? vigencia) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TipoRubro>(
				"SELECT ID, NOMBRE, DESCRIPCION, VIGENCIA FROM TANATOS.TIPO_RUBRO WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)",
				new { vigencia }
			)];
		}

		public async Task Insertar(TipoRubro item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"INSERT INTO TANATOS.TIPO_RUBRO(ID, NOMBRE, DESCRIPCION, VIGENCIA) VALUES (@ID, @NOMBRE, @DESCRIPCION, @VIGENCIA)",
				new { item.Id, item.Nombre, item.Descripcion, item.Vigencia }
			);
		}

		public async Task Actualizar(TipoRubro item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.TIPO_RUBRO SET NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, VIGENCIA = @VIGENCIA WHERE ID = @ID",
				new { item.Nombre, item.Descripcion, item.Vigencia, item.Id }
			);
		}

		public async Task Eliminar(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"DELETE FROM TANATOS.TIPO_RUBRO WHERE ID = @ID",
				new { id }
			);
		}
	}
}
