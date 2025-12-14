using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class NegocioDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<Negocio>> ObtenerPorSub(string sub, bool vigencia = true) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<Negocio>(
				"SELECT ID, SUB, NOMBRE, DIRECCION, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.NEGOCIO WHERE SUB = @SUB AND VIGENCIA = @VIGENCIA",
				new { sub, vigencia }
			)];
		}

		public async Task<long> Insertar(Negocio item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.ExecuteScalarAsync<long>(
				"INSERT INTO TANATOS.NEGOCIO(SUB, NOMBRE, DIRECCION, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @NOMBRE, @DIRECCION, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID",
				new { item.Sub, item.Nombre, item.Direccion, item.FechaCreacion, item.FechaEliminacion, item.Vigencia }
			);
		}

		public async Task Actualizar(Negocio item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.NEGOCIO SET SUB = @SUB, NOMBRE = @NOMBRE, DIRECCION = @DIRECCION, FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, " +
				"VIGENCIA = @VIGENCIA WHERE ID = @ID",
				new { item.Sub, item.Nombre, item.Direccion, item.FechaCreacion, item.FechaEliminacion, item.Vigencia, item.Id }
			);
		}
	}
}
