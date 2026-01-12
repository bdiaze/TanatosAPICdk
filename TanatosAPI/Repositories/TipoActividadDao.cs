using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TipoActividadDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<TipoActividad?> ObtenerPorId(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.QueryFirstOrDefaultAsync<TipoActividad>(
				"SELECT ID, ID_TIPO_RUBRO, NOMBRE, DESCRIPCION, VIGENCIA FROM TANATOS.TIPO_ACTIVIDAD WHERE ID = @ID",
				new { id }
			);
		}

		public async Task<List<TipoActividad>> ObtenerPorVigencia(bool? vigencia) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TipoActividad>(
				"SELECT ID, ID_TIPO_RUBRO, NOMBRE, DESCRIPCION, VIGENCIA FROM TANATOS.TIPO_ACTIVIDAD WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)",
				new { vigencia }
			)];
		}

		public async Task Insertar(TipoActividad item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"INSERT INTO TANATOS.TIPO_ACTIVIDAD(ID, ID_TIPO_RUBRO, NOMBRE, DESCRIPCION, VIGENCIA) VALUES (@ID, @IDTIPORUBRO, @NOMBRE, @DESCRIPCION, @VIGENCIA)",
				new { item.Id, item.IdTipoRubro, item.Nombre, item.Descripcion, item.Vigencia }
			);
		}

		public async Task Actualizar(TipoActividad item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.TIPO_ACTIVIDAD SET ID_TIPO_RUBRO = @IDTIPORUBRO, NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, VIGENCIA = @VIGENCIA WHERE ID = @ID",
				new { item.IdTipoRubro, item.Nombre, item.Descripcion, item.Vigencia, item.Id }
			);
		}

		public async Task Eliminar(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"DELETE FROM TANATOS.TIPO_ACTIVIDAD WHERE ID = @ID",
				new { id }
			);
		}
	}
}
