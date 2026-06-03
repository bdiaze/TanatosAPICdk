using Dapper;
using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class NegocioDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<Negocio>> ObtenerPorSub(string sub, bool vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, NOMBRE, DIRECCION, ID_TIPO_ACTIVIDAD, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.NEGOCIO " +
				"WHERE SUB = @SUB AND VIGENCIA = @VIGENCIA";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", sub);
				command.Parameters.AddWithValue("VIGENCIA", vigencia);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<Negocio> retorno = [];

				while (await reader.ReadAsync()) {
					retorno.Add(new Negocio {
						Id = reader.GetInt64(0),
						Sub = reader.GetString(1),
						Nombre = reader.GetString(2),
						Direccion = await reader.IsDBNullAsync(3) ? null : reader.GetString(3),
						IdTipoActividad = await reader.IsDBNullAsync(4) ? null : reader.GetInt64(4),
						FechaCreacion = reader.GetDateTime(5),
						FechaEliminacion = await reader.IsDBNullAsync(6) ? null : reader.GetDateTime(6),
						Vigencia = reader.GetBoolean(7)
					});
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<long> Insertar(Negocio item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.ExecuteScalarAsync<long>(
				"INSERT INTO TANATOS.NEGOCIO(SUB, NOMBRE, DIRECCION, ID_TIPO_ACTIVIDAD, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @NOMBRE, @DIRECCION, @IDTIPOACTIVIDAD, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID",
				new { item.Sub, item.Nombre, item.Direccion, item.IdTipoActividad, item.FechaCreacion, item.FechaEliminacion, item.Vigencia }
			);
		}

		public async Task Actualizar(Negocio item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.NEGOCIO SET SUB = @SUB, NOMBRE = @NOMBRE, DIRECCION = @DIRECCION, ID_TIPO_ACTIVIDAD = @IDTIPOACTIVIDAD, FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, " +
				"VIGENCIA = @VIGENCIA WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("SUB", item.Sub);
			param.Add("NOMBRE", item.Nombre);
			param.Add("DIRECCION", item.Direccion);
			param.Add("IDTIPOACTIVIDAD", item.IdTipoActividad);
			param.Add("FECHACREACION", item.FechaCreacion);
			param.Add("FECHAELIMINACION", item.FechaEliminacion);
			param.Add("VIGENCIA", item.Vigencia);
			param.Add("ID", item.Id);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
