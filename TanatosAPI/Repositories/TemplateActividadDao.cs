using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class TemplateActividadDao(IDatabaseConnectionHelper connectionHelper) {
		public async Task<List<TemplateActividad>> ObtenerPorTemplate(long idTemplate, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID_TEMPLATE, ID_TIPO_ACTIVIDAD FROM TANATOS.TEMPLATE_ACTIVIDAD WHERE ID_TEMPLATE = @IDTEMPLATE";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDTEMPLATE", idTemplate);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<TemplateActividad> retorno = [];
                while (await reader.ReadAsync()) {
                    retorno.Add(new TemplateActividad {
						IdTemplate = reader.GetInt64(0),
						IdTipoActividad = reader.GetInt64(1),
                    });
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task<List<TemplateActividad>> ObtenerPorActividad(long idTipoActividad, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID_TEMPLATE, ID_TIPO_ACTIVIDAD FROM TANATOS.TEMPLATE_ACTIVIDAD WHERE ID_TIPO_ACTIVIDAD = @IDTIPOACTIVIDAD";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDTIPOACTIVIDAD", idTipoActividad);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<TemplateActividad> retorno = [];
                while (await reader.ReadAsync()) {
                    retorno.Add(new TemplateActividad {
                        IdTemplate = reader.GetInt64(0),
                        IdTipoActividad = reader.GetInt64(1),
                    });
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Insertar(TemplateActividad item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.TEMPLATE_ACTIVIDAD(ID_TEMPLATE, ID_TIPO_ACTIVIDAD) " +
				"VALUES (@IDTEMPLATE, @IDTIPOACTIVIDAD)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDTEMPLATE", item.IdTemplate);
                command.Parameters.AddWithValue("IDTIPOACTIVIDAD", item.IdTipoActividad);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}

		public async Task Eliminar(long idTemplate, long? idTipoActividad, NpgsqlTransaction? transaction = null) {
			string query = "DELETE FROM TANATOS.TEMPLATE_ACTIVIDAD WHERE ID_TEMPLATE = @IDTEMPLATE AND (ID_TIPO_ACTIVIDAD = @IDTIPOACTIVIDAD OR @IDTIPOACTIVIDAD IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDTEMPLATE", idTemplate);
                command.Parameters.AddWithValue("IDTIPOACTIVIDAD", (object?)idTipoActividad ?? DBNull.Value);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}
	}
}
