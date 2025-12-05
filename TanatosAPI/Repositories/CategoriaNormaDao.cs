using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class CategoriaNormaDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<CategoriaNorma?> ObtenerPorId(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.QueryFirstOrDefaultAsync<CategoriaNorma>(
				"SELECT ID, NOMBRE, NOMBRE_CORTO, DESCRIPCION, VIGENCIA FROM TANATOS.CATEGORIA_NORMA WHERE ID = @ID",
				new { id }
			);
		}

		public async Task<List<CategoriaNorma>> ObtenerPorVigencia(bool vigencia) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<CategoriaNorma>(
				"SELECT ID, NOMBRE, NOMBRE_CORTO, DESCRIPCION, VIGENCIA FROM TANATOS.CATEGORIA_NORMA WHERE VIGENCIA = @VIGENCIA",
				new { vigencia }
			)];
		}

		public async Task Insertar(CategoriaNorma item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"INSERT INTO TANATOS.CATEGORIA_NORMA(ID, NOMBRE, NOMBRE_CORTO, DESCRIPCION, VIGENCIA) VALUES (@ID, @NOMBRE, @NOMBRECORTO, @DESCRIPCION, @VIGENCIA)",
				new { item.Id, item.Nombre, item.NombreCorto, item.Descripcion, item.Vigencia }
			);
		}

		public async Task Actualizar(CategoriaNorma item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.CATEGORIA_NORMA SET NOMBRE = @NOMBRE, NOMBRE_CORTO = @NOMBRECORTO, DESCRIPCION = @DESCRIPCION, VIGENCIA = @VIGENCIA WHERE ID = @ID",
				new { item.Nombre, item.NombreCorto, item.Descripcion, item.Vigencia, item.Id }
			);
		}

		public async Task Eliminar(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"DELETE FROM TANATOS.CATEGORIA_NORMA WHERE ID = @ID",
				new { id }
			);
		}
	}
}
