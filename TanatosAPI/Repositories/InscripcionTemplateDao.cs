using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	public class InscripcionTemplateDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<InscripcionTemplate>> ObtenerPorSub(string sub, long idNegocio, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT SUB, ID_NEGOCIO, ID_TEMPLATE, FECHA_ACTIVACION, FECHA_DESACTIVACION, VIGENCIA " +
				"FROM TANATOS.INSCRIPCION_TEMPLATE " +
                "WHERE SUB = @SUB AND ID_NEGOCIO = @IDNEGOCIO AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);

                command.Parameters.AddWithValue("SUB", sub);
                command.Parameters.AddWithValue("IDNEGOCIO", idNegocio);
                command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<InscripcionTemplate> retorno = [];
                while (await reader.ReadAsync()) {
                    retorno.Add(new InscripcionTemplate {
						Sub = reader.GetString(0),
						IdNegocio = reader.GetInt64(1),
						IdTemplate = reader.GetInt64(2),
						FechaActivacion = reader.GetDateTime(3),
						FechaDesactivacion = await reader.IsDBNullAsync(4) ? null : reader.GetDateTime(4),
						Vigencia = reader.GetBoolean(5)
                    });
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Insertar(InscripcionTemplate item, NpgsqlTransaction? transaction = null) {
			string query = 
				"INSERT INTO TANATOS.INSCRIPCION_TEMPLATE(SUB, ID_NEGOCIO, ID_TEMPLATE, FECHA_ACTIVACION, FECHA_DESACTIVACION, VIGENCIA) " +
				"VALUES (@SUB, @IDNEGOCIO, @IDTEMPLATE, @FECHAACTIVACION, @FECHADESACTIVACION, @VIGENCIA)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("IDNEGOCIO", item.IdNegocio);
                command.Parameters.AddWithValue("IDTEMPLATE", item.IdTemplate);
                command.Parameters.AddWithValue("FECHAACTIVACION", item.FechaActivacion);
                command.Parameters.AddWithValue("FECHADESACTIVACION", (object?)item.FechaDesactivacion ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}

		public async Task Actualizar(InscripcionTemplate item, NpgsqlTransaction? transaction = null) {
			string query = 
				"UPDATE TANATOS.INSCRIPCION_TEMPLATE SET FECHA_ACTIVACION = @FECHAACTIVACION, " +
                "FECHA_DESACTIVACION = @FECHADESACTIVACION, VIGENCIA = @VIGENCIA " +
				"WHERE SUB = @SUB AND ID_NEGOCIO = @IDNEGOCIO AND ID_TEMPLATE = @IDTEMPLATE";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("FECHAACTIVACION", item.FechaActivacion);
                command.Parameters.AddWithValue("FECHADESACTIVACION", (object?)item.FechaDesactivacion ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("IDNEGOCIO", item.IdNegocio);
                command.Parameters.AddWithValue("IDTEMPLATE", item.IdTemplate);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}
	}
}
