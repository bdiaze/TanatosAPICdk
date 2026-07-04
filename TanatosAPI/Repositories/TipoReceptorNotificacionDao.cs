using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class TipoReceptorNotificacionDao(IDatabaseConnectionHelper connectionHelper) : ITipoReceptorNotificacionDao {

        public async Task<TipoReceptorNotificacion?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, NOMBRE, REGEX_VALIDACION, REQUIERE_PLAN_EMPRESA, VIGENCIA FROM TANATOS.TIPO_RECEPTOR_NOTIFICACION WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				TipoReceptorNotificacion? retorno = null;

				if (await reader.ReadAsync()) {
					retorno = new TipoReceptorNotificacion {
						Id = reader.GetInt64(0),
                        Nombre = reader.GetString(1),
                        RegexValidacion = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
                        RequierePlanEmpresa = reader.GetBoolean(3),
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

        public async Task<List<TipoReceptorNotificacion>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, NOMBRE, REGEX_VALIDACION, REQUIERE_PLAN_EMPRESA, VIGENCIA " +
                "FROM TANATOS.TIPO_RECEPTOR_NOTIFICACION WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<TipoReceptorNotificacion> retorno = [];
                while (await reader.ReadAsync()) {
                    retorno.Add(new TipoReceptorNotificacion {
                        Id = reader.GetInt64(0),
                        Nombre = reader.GetString(1),
                        RegexValidacion = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
                        RequierePlanEmpresa = reader.GetBoolean(3),
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

        public async Task Insertar(TipoReceptorNotificacion item, NpgsqlTransaction? transaction = null) {
            string query =
                "INSERT INTO TANATOS.TIPO_RECEPTOR_NOTIFICACION(ID, NOMBRE, REGEX_VALIDACION, REQUIERE_PLAN_EMPRESA, VIGENCIA) " +
                "VALUES (@ID, @NOMBRE, @REGEXVALIDACION, @REQUIEREPLANEMPRESA, @VIGENCIA)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", item.Id);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("REGEXVALIDACION", (object?)item.RegexValidacion ?? DBNull.Value);
                command.Parameters.AddWithValue("REQUIEREPLANEMPRESA", item.RequierePlanEmpresa);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

        public async Task Actualizar(TipoReceptorNotificacion item, NpgsqlTransaction? transaction = null) {
            string query =
                "UPDATE TANATOS.TIPO_RECEPTOR_NOTIFICACION SET NOMBRE = @NOMBRE, REGEX_VALIDACION = @REGEXVALIDACION, " +
                "REQUIERE_PLAN_EMPRESA = @REQUIEREPLANEMPRESA, VIGENCIA = @VIGENCIA WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("REGEXVALIDACION", (object?)item.RegexValidacion ?? DBNull.Value);
                command.Parameters.AddWithValue("REQUIEREPLANEMPRESA", item.RequierePlanEmpresa);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                command.Parameters.AddWithValue("ID", item.Id);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

        public async Task Eliminar(long id, NpgsqlTransaction? transaction = null) {
            string query =
                "DELETE FROM TANATOS.TIPO_RECEPTOR_NOTIFICACION WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", id);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

	}
}
