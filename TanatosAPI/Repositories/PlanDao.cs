using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	public class PlanDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<Plan?> Obtener(long id, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, NOMBRE, PRECIO, DURACION_MESES, SUSCRIPCION_UNICA, FLOW_PLAN_ID, VIGENCIA " +
                "FROM TANATOS.PLAN WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", id);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                Plan? retorno = null;
                if (await reader.ReadAsync()) {
                    retorno = new Plan {
                        Id = reader.GetInt64(0),
						Nombre = reader.GetString(1),
						Precio = reader.GetDecimal(2),
						DuracionMeses = reader.GetInt32(3),
						SuscripcionUnica = reader.GetBoolean(4),
						FlowPlanId = await reader.IsDBNullAsync(5) ? null : reader.GetString(5),
						Vigencia = reader.GetBoolean(6),						
                    };
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task<List<Plan>> ObtenerPorVigencia(bool? vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
                "SELECT ID, NOMBRE, PRECIO, DURACION_MESES, SUSCRIPCION_UNICA, FLOW_PLAN_ID, VIGENCIA " +
                "FROM TANATOS.PLAN WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<Plan> retorno = [];
                while (await reader.ReadAsync()) {
                    retorno.Add(new Plan {
                        Id = reader.GetInt64(0),
                        Nombre = reader.GetString(1),
                        Precio = reader.GetDecimal(2),
                        DuracionMeses = reader.GetInt32(3),
                        SuscripcionUnica = reader.GetBoolean(4),
                        FlowPlanId = await reader.IsDBNullAsync(5) ? null : reader.GetString(5),
                        Vigencia = reader.GetBoolean(6),
                    });
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Insertar(Plan item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.PLAN(ID, NOMBRE, PRECIO, DURACION_MESES, SUSCRIPCION_UNICA, FLOW_PLAN_ID, VIGENCIA) " +
				"VALUES (@ID, @NOMBRE, @PRECIO, @DURACIONMESES, @SUSCRIPCIONUNICA, @FLOWPLANID, @VIGENCIA)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", item.Id);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("PRECIO", item.Precio);
                command.Parameters.AddWithValue("DURACIONMESES", item.DuracionMeses);
                command.Parameters.AddWithValue("SUSCRIPCIONUNICA", item.SuscripcionUnica);
                command.Parameters.AddWithValue("FLOWPLANID", (object?)item.FlowPlanId ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}

		public async Task Actualizar(Plan item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.PLAN SET NOMBRE = @NOMBRE, PRECIO = @PRECIO, DURACION_MESES = @DURACIONMESES, " +
                "SUSCRIPCION_UNICA = @SUSCRIPCIONUNICA, " +
				"FLOW_PLAN_ID = @FLOWPLANID, VIGENCIA = @VIGENCIA WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("PRECIO", item.Precio);
                command.Parameters.AddWithValue("DURACIONMESES", item.DuracionMeses);
                command.Parameters.AddWithValue("SUSCRIPCIONUNICA", item.SuscripcionUnica);
                command.Parameters.AddWithValue("FLOWPLANID", (object?)item.FlowPlanId ?? DBNull.Value);
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
				"DELETE FROM TANATOS.PLAN WHERE ID = @ID";

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
