using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class SuscripcionDao(IDatabaseConnectionHelper connectionHelper) {

		public async Task<Suscripcion?> Obtener(long id, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, SUB, ID_PLAN, FECHA_INICIO, FECHA_EXPIRACION, FECHA_CANCELACION, ESTADO, FLOW_CUSTOMER_ID, FLOW_SUBSCRIPTION_ID, " +
                "FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.SUSCRIPCION WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", id);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                Suscripcion? retorno = null;
                if (await reader.ReadAsync()) {
                    retorno = new Suscripcion {
                        Id = reader.GetInt64(0),
                        Sub = reader.GetString(1),
                        IdPlan = reader.GetInt64(2),
                        FechaInicio = await reader.IsDBNullAsync(3) ? null : reader.GetDateTime(3),
                        FechaExpiracion = await reader.IsDBNullAsync(4) ? null : reader.GetDateTime(4),
                        FechaCancelacion = await reader.IsDBNullAsync(5) ? null : reader.GetDateTime(5),
                        Estado = reader.GetInt16(6),
                        FlowCustomerId = await reader.IsDBNullAsync(7) ? null : reader.GetString(7),
                        FlowSubscriptionId = await reader.IsDBNullAsync(8) ? null : reader.GetString(8),
                        FechaCreacion = reader.GetDateTime(9),
                        FechaEliminacion = await reader.IsDBNullAsync(10) ? null : reader.GetDateTime(10),
                        Vigencia = reader.GetBoolean(11)
                    };
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task<Suscripcion?> ObtenerPorFlowSubscriptionId(string flowSubscriptionId, NpgsqlTransaction? transaction = null) {
			string query =
                "SELECT ID, SUB, ID_PLAN, FECHA_INICIO, FECHA_EXPIRACION, FECHA_CANCELACION, ESTADO, FLOW_CUSTOMER_ID, FLOW_SUBSCRIPTION_ID, " +
                "FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.SUSCRIPCION WHERE FLOW_SUBSCRIPTION_ID = @FLOWSUBSCRIPTIONID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("FLOWSUBSCRIPTIONID", flowSubscriptionId);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                Suscripcion? retorno = null;
                if (await reader.ReadAsync()) {
                    retorno = new Suscripcion {
                        Id = reader.GetInt64(0),
                        Sub = reader.GetString(1),
                        IdPlan = reader.GetInt64(2),
                        FechaInicio = await reader.IsDBNullAsync(3) ? null : reader.GetDateTime(3),
                        FechaExpiracion = await reader.IsDBNullAsync(4) ? null : reader.GetDateTime(4),
                        FechaCancelacion = await reader.IsDBNullAsync(5) ? null : reader.GetDateTime(5),
                        Estado = reader.GetInt16(6),
                        FlowCustomerId = await reader.IsDBNullAsync(7) ? null : reader.GetString(7),
                        FlowSubscriptionId = await reader.IsDBNullAsync(8) ? null : reader.GetString(8),
                        FechaCreacion = reader.GetDateTime(9),
                        FechaEliminacion = await reader.IsDBNullAsync(10) ? null : reader.GetDateTime(10),
                        Vigencia = reader.GetBoolean(11)
                    };
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task<List<Suscripcion>> ObtenerPorSub(string sub, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_PLAN, FECHA_INICIO, FECHA_EXPIRACION, FECHA_CANCELACION, ESTADO, FLOW_CUSTOMER_ID, FLOW_SUBSCRIPTION_ID, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.SUSCRIPCION WHERE SUB = @SUB AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", sub);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<Suscripcion> retorno = [];

				while (await reader.ReadAsync()) {
					retorno.Add(new Suscripcion {
						Id = reader.GetInt64(0),
						Sub = reader.GetString(1),
						IdPlan = reader.GetInt64(2),
						FechaInicio = await reader.IsDBNullAsync(3) ? null : reader.GetDateTime(3),
						FechaExpiracion = await reader.IsDBNullAsync(4) ? null : reader.GetDateTime(4),
						FechaCancelacion = await reader.IsDBNullAsync(5) ? null : reader.GetDateTime(5),
						Estado = reader.GetInt16(6),
						FlowCustomerId = await reader.IsDBNullAsync(7) ? null : reader.GetString(7),
						FlowSubscriptionId = await reader.IsDBNullAsync(8) ? null : reader.GetString(8),
						FechaCreacion = reader.GetDateTime(9),
						FechaEliminacion = await reader.IsDBNullAsync(10) ? null : reader.GetDateTime(10),
						Vigencia = reader.GetBoolean(11)
					});
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<long> Insertar(Suscripcion item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.SUSCRIPCION(SUB, ID_PLAN, FECHA_INICIO, FECHA_EXPIRACION, FECHA_CANCELACION, ESTADO, FLOW_CUSTOMER_ID, FLOW_SUBSCRIPTION_ID, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @IDPLAN, @FECHAINICIO, @FECHAEXPIRACION, @FECHACANCELACION, @ESTADO, @FLOWCUSTOMERID, @FLOWSUBSCRIPTIONID, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("IDPLAN", item.IdPlan);
                command.Parameters.AddWithValue("FECHAINICIO", (object?)item.FechaInicio ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHAEXPIRACION", (object?)item.FechaExpiracion ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHACANCELACION", (object?)item.FechaCancelacion ?? DBNull.Value);
                command.Parameters.AddWithValue("ESTADO", item.Estado);
                command.Parameters.AddWithValue("FLOWCUSTOMERID", (object?)item.FlowCustomerId ?? DBNull.Value);
                command.Parameters.AddWithValue("FLOWSUBSCRIPTIONID", (object?)item.FlowSubscriptionId ?? DBNull.Value);
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

		public async Task Actualizar(Suscripcion item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.SUSCRIPCION SET SUB = @SUB, ID_PLAN = @IDPLAN, FECHA_INICIO = @FECHAINICIO, FECHA_EXPIRACION = @FECHAEXPIRACION, " +
				"FECHA_CANCELACION = @FECHACANCELACION, ESTADO = @ESTADO, FLOW_CUSTOMER_ID = @FLOWCUSTOMERID, FLOW_SUBSCRIPTION_ID = @FLOWSUBSCRIPTIONID, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("IDPLAN", item.IdPlan);
                command.Parameters.AddWithValue("FECHAINICIO", (object?)item.FechaInicio ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHAEXPIRACION", (object?)item.FechaExpiracion ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHACANCELACION", (object?)item.FechaCancelacion ?? DBNull.Value);
                command.Parameters.AddWithValue("ESTADO", item.Estado);
                command.Parameters.AddWithValue("FLOWCUSTOMERID", (object?)item.FlowCustomerId ?? DBNull.Value);
                command.Parameters.AddWithValue("FLOWSUBSCRIPTIONID", (object?)item.FlowSubscriptionId ?? DBNull.Value);
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
