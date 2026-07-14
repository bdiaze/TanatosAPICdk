using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	[ExcludeFromCodeCoverage]
	public class EvaluacionDao(IDatabaseConnectionHelper connectionHelper) : IEvaluacionDao {
		public async Task<List<Evaluacion>> Obtener(DateTime? fechaDesde = null, DateTime? fechasHasta = null, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, PUNTAJE, COMENTARIO, FECHA_CREACION " +
				"FROM TANATOS.EVALUACION " +
				"WHERE (FECHA_CREACION >= @FECHADESDE OR @FECHADESDE IS NULL) AND (FECHA_CREACION <= @FECHAHASTA OR @FECHAHASTA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("FECHADESDE", (object?)fechaDesde ?? DBNull.Value);
				command.Parameters.AddWithValue("FECHAHASTA", (object?)fechasHasta ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<Evaluacion> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new Evaluacion {
						Id = reader.GetInt64(0),
						Sub = reader.GetString(1),
						Puntaje = reader.GetInt16(2),
						Comentario = await reader.IsDBNullAsync(3) ? null : reader.GetString(3),
						FechaCreacion = reader.GetDateTime(4)
					});
				}
				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<long> Insertar(Evaluacion item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.EVALUACION(SUB, PUNTAJE, COMENTARIO, FECHA_CREACION) " +
				"VALUES (@SUB, @PUNTAJE, @COMENTARIO, @FECHACREACION) " +
				"RETURNING ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", item.Sub);
				command.Parameters.AddWithValue("PUNTAJE", item.Puntaje);
				command.Parameters.AddWithValue("COMENTARIO", (object?)item.Comentario ?? DBNull.Value);
				command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);
				return Convert.ToInt64(await command.ExecuteScalarAsync());
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}
	}
}
