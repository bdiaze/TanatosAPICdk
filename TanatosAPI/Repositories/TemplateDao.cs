using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class TemplateDao(IDatabaseConnectionHelper connectionHelper) : ITemplateDao {
		public async Task<Template?> ObtenerPorId(long idTemplate, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, ID_TEMPLATE_PADRE, NOMBRE, DESCRIPCION, REQUIERE_PLAN_EMPRESA, VIGENCIA " +
                "FROM TANATOS.TEMPLATE WHERE ID = @IDTEMPLATE";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDTEMPLATE", idTemplate);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                Template? retorno = null;
                if (await reader.ReadAsync()) {
                    retorno = new Template {
                        Id = reader.GetInt64(0),
						IdTemplatePadre = await reader.IsDBNullAsync(1) ? null : reader.GetInt64(1),
						Nombre = reader.GetString(2),
						Descripcion = reader.GetString(3),
                        RequierePlanEmpresa = reader.GetBoolean(4),
                        Vigencia = reader.GetBoolean(5)
                    };
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task<List<Template>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, ID_TEMPLATE_PADRE, NOMBRE, DESCRIPCION, REQUIERE_PLAN_EMPRESA, VIGENCIA " +
                "FROM TANATOS.TEMPLATE WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<Template> retorno = [];
                while (await reader.ReadAsync()) {
                    retorno.Add(new Template {
                        Id = reader.GetInt64(0),
                        IdTemplatePadre = await reader.IsDBNullAsync(1) ? null : reader.GetInt64(1),
                        Nombre = reader.GetString(2),
                        Descripcion = reader.GetString(3),
                        RequierePlanEmpresa = reader.GetBoolean(4),
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

		public async Task Insertar(Template item, NpgsqlTransaction? transaction = null) {
			string query = "INSERT INTO TANATOS.TEMPLATE(ID, ID_TEMPLATE_PADRE, NOMBRE, DESCRIPCION, REQUIERE_PLAN_EMPRESA, VIGENCIA) " +
				"VALUES (@ID, @IDTEMPLATEPADRE, @NOMBRE, @DESCRIPCION, @REQUIEREPLANEMPRESA, @VIGENCIA)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("ID", item.Id);
                command.Parameters.AddWithValue("IDTEMPLATEPADRE", (object?)item.IdTemplatePadre ?? DBNull.Value);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("DESCRIPCION", item.Descripcion);
                command.Parameters.AddWithValue("REQUIEREPLANEMPRESA", item.RequierePlanEmpresa);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}

		public async Task Actualizar(Template item, NpgsqlTransaction? transaction = null) {
			string query = "UPDATE TANATOS.TEMPLATE SET ID_TEMPLATE_PADRE = @IDTEMPLATEPADRE, NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, " +
				"REQUIERE_PLAN_EMPRESA = @REQUIEREPLANEMPRESA, VIGENCIA = @VIGENCIA WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDTEMPLATEPADRE", (object?)item.IdTemplatePadre ?? DBNull.Value);
                command.Parameters.AddWithValue("NOMBRE", item.Nombre);
                command.Parameters.AddWithValue("DESCRIPCION", item.Descripcion);
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
			string query = "DELETE FROM TANATOS.TEMPLATE WHERE ID = @ID";

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
