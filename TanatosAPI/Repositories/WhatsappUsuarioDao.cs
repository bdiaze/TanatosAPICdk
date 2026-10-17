using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	public class WhatsappUsuarioDao(IDatabaseConnectionHelper connectionHelper) : IWhatsappUsuarioDao {
		public async Task<WhatsappUsuario?> Obtener(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, WHATSAPP, ID_ACCESO_VALIDACION, FECHA_VALIDACION, VALIDADO, HERMES_ID_MENSAJE, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.WHATSAPP_USUARIO " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				WhatsappUsuario? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new WhatsappUsuario {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						Whatsapp = reader.GetString(reader.GetOrdinal("WHATSAPP")),
						IdAccesoValidacion = reader.GetInt64(reader.GetOrdinal("ID_ACCESO_VALIDACION")),
						FechaValidacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_VALIDACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_VALIDACION")),
						Validado = reader.GetBoolean(reader.GetOrdinal("VALIDADO")),
						HermesIdMensaje = await reader.IsDBNullAsync(reader.GetOrdinal("HERMES_ID_MENSAJE")) ? null : reader.GetString(reader.GetOrdinal("HERMES_ID_MENSAJE")),
						FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
						FechaEliminacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_ELIMINACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_ELIMINACION")),
						Vigencia = reader.GetBoolean(reader.GetOrdinal("VIGENCIA"))
					};
				}
				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<List<WhatsappUsuario>> ObtenerPorSub(string sub, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, WHATSAPP, ID_ACCESO_VALIDACION, FECHA_VALIDACION, VALIDADO, HERMES_ID_MENSAJE, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.WHATSAPP_USUARIO " +
				"WHERE SUB = @SUB";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", sub);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				List<WhatsappUsuario> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new WhatsappUsuario {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						Whatsapp = reader.GetString(reader.GetOrdinal("WHATSAPP")),
						IdAccesoValidacion = reader.GetInt64(reader.GetOrdinal("ID_ACCESO_VALIDACION")),
						FechaValidacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_VALIDACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_VALIDACION")),
						Validado = reader.GetBoolean(reader.GetOrdinal("VALIDADO")),
						HermesIdMensaje = await reader.IsDBNullAsync(reader.GetOrdinal("HERMES_ID_MENSAJE")) ? null : reader.GetString(reader.GetOrdinal("HERMES_ID_MENSAJE")),
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

		public async Task<long> Insertar(WhatsappUsuario item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.WHATSAPP_USUARIO(SUB, WHATSAPP, ID_ACCESO_VALIDACION, FECHA_VALIDACION, VALIDADO, HERMES_ID_MENSAJE, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @WHATSAPP, @IDACCESOVALIDACION, @FECHAVALIDACION, @VALIDADO, @HERMESIDMENSAJE, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", item.Sub);
				command.Parameters.AddWithValue("WHATSAPP", item.Whatsapp);
				command.Parameters.AddWithValue("IDACCESOVALIDACION", item.IdAccesoValidacion);
				command.Parameters.AddWithValue("FECHAVALIDACION", (object?)item.FechaValidacion ?? DBNull.Value);
				command.Parameters.AddWithValue("VALIDADO", item.Validado);
				command.Parameters.AddWithValue("HERMESIDMENSAJE", (object?)item.HermesIdMensaje ?? DBNull.Value);
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

		public async Task Actualizar(WhatsappUsuario item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.WHATSAPP_USUARIO SET SUB = @SUB, WHATSAPP = @WHATSAPP, ID_ACCESO_VALIDACION = @IDACCESOVALIDACION, " +
				"FECHA_VALIDACION = @FECHAVALIDACION, VALIDADO = @VALIDADO, HERMES_ID_MENSAJE = @HERMESIDMENSAJE, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", item.Sub);
				command.Parameters.AddWithValue("WHATSAPP", item.Whatsapp);
				command.Parameters.AddWithValue("IDACCESOVALIDACION", item.IdAccesoValidacion);
				command.Parameters.AddWithValue("FECHAVALIDACION", (object?)item.FechaValidacion ?? DBNull.Value);
				command.Parameters.AddWithValue("VALIDADO", item.Validado);
				command.Parameters.AddWithValue("HERMESIDMENSAJE", (object?)item.HermesIdMensaje ?? DBNull.Value);
				command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);
				command.Parameters.AddWithValue("FECHAELIMINACION", (object?)item.FechaEliminacion ?? DBNull.Value);
				command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
				command.Parameters.AddWithValue("ID", item.Id);
				await command.ExecuteNonQueryAsync();
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}
	}
}
