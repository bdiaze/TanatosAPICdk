using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	public class TemplateNormaFiscalizadorDao(DatabaseConnectionHelper connectionHelper) {
        public async Task<List<TemplateNormaFiscalizador>> ObtenerPorTemplateNorma(long idTemplate, long? idNorma = null, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID_TEMPLATE, ID_NORMA, ID_TIPO_FISCALIZADOR FROM TANATOS.TEMPLATE_NORMA_FISCALIZADOR " +
				"WHERE ID_TEMPLATE = @IDTEMPLATE AND (ID_NORMA = @IDNORMA OR @IDNORMA IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDTEMPLATE", idTemplate);
                command.Parameters.AddWithValue("IDNORMA", (object?)idNorma ?? DBNull.Value);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<TemplateNormaFiscalizador> retorno = [];
                while (await reader.ReadAsync()) {
                    retorno.Add(new TemplateNormaFiscalizador {
                        IdTemplate = reader.GetInt64(0),
                        IdNorma = reader.GetInt64(1),
                        IdTipoFiscalizador = reader.GetInt64(2),
                    });
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

        public async Task<List<TemplateNormaFiscalizador>> ObtenerPorFiscalizador(long idTipoFiscalizador, NpgsqlTransaction? transaction = null) {
			string query =
                "SELECT ID_TEMPLATE, ID_NORMA, ID_TIPO_FISCALIZADOR " +
                "FROM TANATOS.TEMPLATE_NORMA_FISCALIZADOR WHERE ID_TIPO_FISCALIZADOR = @IDTIPOFISCALIZADOR";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDTIPOFISCALIZADOR", idTipoFiscalizador);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<TemplateNormaFiscalizador> retorno = [];
                while (await reader.ReadAsync()) {
                    retorno.Add(new TemplateNormaFiscalizador {
                        IdTemplate = reader.GetInt64(0),
                        IdNorma = reader.GetInt64(1),
                        IdTipoFiscalizador = reader.GetInt64(2),
                    });
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task Insertar(TemplateNormaFiscalizador item, NpgsqlTransaction? transaction = null) {
			string query = "INSERT INTO TANATOS.TEMPLATE_NORMA_FISCALIZADOR(ID_TEMPLATE, ID_NORMA, ID_TIPO_FISCALIZADOR) " +
                "VALUES (@IDTEMPLATE, @IDNORMA, @IDTIPOFISCALIZADOR)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDTEMPLATE", item.IdTemplate);
                command.Parameters.AddWithValue("IDNORMA", item.IdNorma);
                command.Parameters.AddWithValue("IDTIPOFISCALIZADOR", item.IdTipoFiscalizador);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}

		public async Task Eliminar(long idTemplate, long? idNorma, long? idTipoFiscalizador, NpgsqlTransaction? transaction = null) {
			string query = "DELETE FROM TANATOS.TEMPLATE_NORMA_FISCALIZADOR " +
                "WHERE ID_TEMPLATE = @IDTEMPLATE AND (ID_NORMA = @IDNORMA OR @IDNORMA IS NULL) AND (ID_TIPO_FISCALIZADOR = @IDTIPOFISCALIZADOR OR @IDTIPOFISCALIZADOR IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDTEMPLATE", idTemplate);
                command.Parameters.AddWithValue("IDNORMA", (object?)idNorma ?? DBNull.Value);
                command.Parameters.AddWithValue("IDTIPOFISCALIZADOR", (object?)idTipoFiscalizador ?? DBNull.Value);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}
	}
}
