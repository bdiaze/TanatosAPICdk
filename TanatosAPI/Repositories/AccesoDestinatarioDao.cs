using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	public class AccesoDestinatarioDao(IDatabaseConnectionHelper connectionHelper) : IAccesoDestinatarioDao {
		public async Task<List<AccesoDestinatario>> ObtenerPorIdAcceso(long idAcceso, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_ACCESO, ID_DESTINATARIO_NOTIFICACION, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.ACCESO_DESTINATARIO " +
				"WHERE ID_ACCESO = @IDACCESO";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDACCESO", idAcceso);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<AccesoDestinatario> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new AccesoDestinatario {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						IdAcceso = reader.GetInt64(reader.GetOrdinal("ID_ACCESO")),
						IdDestinatarioNotificacion = reader.GetInt64(reader.GetOrdinal("ID_DESTINATARIO_NOTIFICACION")),
						FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
						FechaEliminacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_ELIMINACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_ELIMINACION")),
						Vigencia = reader.GetBoolean(reader.GetOrdinal("VIGENCIA"))
					});
				}
				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<long> Insertar(AccesoDestinatario item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.ACCESO_DESTINATARIO(ID_ACCESO, ID_DESTINATARIO_NOTIFICACION, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@IDACCESO, @IDDESTINATARIONOTIFICACION, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDACCESO", item.IdAcceso);
				command.Parameters.AddWithValue("IDDESTINATARIONOTIFICACION", item.IdDestinatarioNotificacion);
				command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);
				command.Parameters.AddWithValue("FECHAELIMINACION", (object?)item.FechaEliminacion ?? DBNull.Value);
				command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
				return Convert.ToInt64(await command.ExecuteScalarAsync());
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}
	}
}
