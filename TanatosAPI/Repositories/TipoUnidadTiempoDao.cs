using Dapper;
using Npgsql;
using System.Data;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TipoUnidadTiempoDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<TipoUnidadTiempo?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, NOMBRE, NOMBRE_PLURAL, CANT_SEGUNDOS, VIGENCIA FROM TANATOS.TIPO_UNIDAD_TIEMPO " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("ID", id);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				TipoUnidadTiempo? retorno = null;

				if (await reader.ReadAsync()) {
					retorno = new TipoUnidadTiempo {
						Id = reader.GetInt64(0),
						Nombre = reader.GetString(1),
						NombrePlural = reader.IsDBNull(2) ? null : reader.GetString(2),
						CantSegundos = reader.GetInt64(3),
						Vigencia = reader.GetBoolean(4)
					};
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<List<TipoUnidadTiempo>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
			string query =
                "SELECT ID, NOMBRE, NOMBRE_PLURAL, CANT_SEGUNDOS, VIGENCIA FROM TANATOS.TIPO_UNIDAD_TIEMPO " +
				"WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<TipoUnidadTiempo> retorno = [];

				while (await reader.ReadAsync()) {
					retorno.Add(new TipoUnidadTiempo {
						Id = reader.GetInt64(0),
						Nombre = reader.GetString(1),
                        NombrePlural = reader.IsDBNull(2) ? null : reader.GetString(2),
                        CantSegundos = reader.GetInt64(3),
						Vigencia = reader.GetBoolean(4)
					});
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task Insertar(TipoUnidadTiempo item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
                "INSERT INTO TANATOS.TIPO_UNIDAD_TIEMPO(ID, NOMBRE, NOMBRE_PLURAL, CANT_SEGUNDOS, VIGENCIA) VALUES (@ID, @NOMBRE, @NOMBREPLURAL, @CANTSEGUNDOS, @VIGENCIA)",
				new { item.Id, item.Nombre, item.NombrePlural, item.CantSegundos, item.Vigencia }
			);
		}

		public async Task Actualizar(TipoUnidadTiempo item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
                "UPDATE TANATOS.TIPO_UNIDAD_TIEMPO SET NOMBRE = @NOMBRE, NOMBRE_PLURAL = @NOMBREPLURAL, CANT_SEGUNDOS = @CANTSEGUNDOS, VIGENCIA = @VIGENCIA WHERE ID = @ID",
				new { item.Nombre, item.NombrePlural, item.CantSegundos, item.Vigencia, item.Id }
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
