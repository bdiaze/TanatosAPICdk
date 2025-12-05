using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TipoUnidadTiempoDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<TipoUnidadTiempo?> ObtenerPorId(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.QueryFirstOrDefaultAsync<TipoUnidadTiempo>(
				"SELECT ID, NOMBRE, CANT_SEGUNDOS, VIGENCIA FROM TANATOS.TIPO_UNIDAD_TIEMPO WHERE ID = @ID",
				new { id }
			);
		}

		public async Task<List<TipoUnidadTiempo>> ObtenerPorVigencia(bool vigencia) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TipoUnidadTiempo>(
				"SELECT ID, NOMBRE, CANT_SEGUNDOS, VIGENCIA FROM TANATOS.TIPO_UNIDAD_TIEMPO WHERE VIGENCIA = @VIGENCIA",
				new { vigencia }
			)];
		}

		public async Task Insertar(TipoUnidadTiempo item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"INSERT INTO TANATOS.TIPO_UNIDAD_TIEMPO(ID, NOMBRE, CANT_SEGUNDOS, VIGENCIA) VALUES (@ID, @NOMBRE, @CANTSEGUNDOS, @VIGENCIA)",
				new { item.Id, item.Nombre, item.CantSegundos, item.Vigencia }
			);
		}

		public async Task Actualizar(TipoUnidadTiempo item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.TIPO_UNIDAD_TIEMPO SET NOMBRE = @NOMBRE, CANT_SEGUNDOS = @CANTSEGUNDOS, VIGENCIA = @VIGENCIA WHERE ID = @ID",
				new { item.Nombre, item.CantSegundos, item.Vigencia, item.Id }
			);
		}

		public async Task Eliminar(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"DELETE FROM TANATOS.TIPO_UNIDAD_TIEMPO WHERE ID = @ID",
				new { id }
			);
		}
	}
}
