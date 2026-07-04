using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class FiscalizadorNormaSuscritaDao(IDatabaseConnectionHelper connectionHelper) : IFiscalizadorNormaSuscritaDao {
		public async Task<List<FiscalizadorNormaSuscrita>> ObtenerPorNormaSuscrita(long idNormaSuscrita, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_NORMA_SUSCRITA, ID_TIPO_FISCALIZADOR, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.FISCALIZADOR_NORMA_SUSCRITA " +
				"WHERE ID_NORMA_SUSCRITA = @IDNORMASUSCRITA AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("IDNORMASUSCRITA", idNormaSuscrita);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<FiscalizadorNormaSuscrita> retorno = [];

				while (await reader.ReadAsync()) {
					retorno.Add(new FiscalizadorNormaSuscrita {
						Id = reader.GetInt64(0),
						IdNormaSuscrita = reader.GetInt64(1),
						IdTipoFiscalizador = reader.GetInt64(2),
						FechaCreacion = await reader.IsDBNullAsync(3) ? null : reader.GetDateTime(3),
						FechaEliminacion = await reader.IsDBNullAsync(4) ? null : reader.GetDateTime(4),
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

		public async Task<long> Insertar(FiscalizadorNormaSuscrita item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.FISCALIZADOR_NORMA_SUSCRITA(ID_NORMA_SUSCRITA, ID_TIPO_FISCALIZADOR, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@IDNORMASUSCRITA, @IDTIPOFISCALIZADOR, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDNORMASUSCRITA", item.IdNormaSuscrita);
                command.Parameters.AddWithValue("IDTIPOFISCALIZADOR", item.IdTipoFiscalizador);
                command.Parameters.AddWithValue("FECHACREACION", (object?)item.FechaCreacion ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHAELIMINACION", (object?)item.FechaEliminacion ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                return Convert.ToInt64(await command.ExecuteScalarAsync());
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}

		public async Task Actualizar(FiscalizadorNormaSuscrita item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.FISCALIZADOR_NORMA_SUSCRITA SET ID_NORMA_SUSCRITA = @IDNORMASUSCRITA, " +
				"ID_TIPO_FISCALIZADOR = @IDTIPOFISCALIZADOR, FECHA_CREACION = @FECHACREACION, " +
				"FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDNORMASUSCRITA", item.IdNormaSuscrita);
                command.Parameters.AddWithValue("IDTIPOFISCALIZADOR", item.IdTipoFiscalizador);
                command.Parameters.AddWithValue("FECHACREACION", (object?)item.FechaCreacion ?? DBNull.Value);
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
