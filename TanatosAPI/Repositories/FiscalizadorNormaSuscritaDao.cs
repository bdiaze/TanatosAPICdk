using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class FiscalizadorNormaSuscritaDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<FiscalizadorNormaSuscrita>> ObtenerPorNormaSuscrita(long idNormaSuscrita, bool vigencia = true) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<FiscalizadorNormaSuscrita>(
				"SELECT ID, ID_NORMA_SUSCRITA, ID_TIPO_FISCALIZADOR, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.FISCALIZADOR_NORMA_SUSCRITA " +
				"WHERE ID_NORMA_SUSCRITA = @IDNORMASUSCRITA AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)",
				new { idNormaSuscrita, vigencia }
			)];
		}

		public async Task<long> Insertar(FiscalizadorNormaSuscrita item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.FISCALIZADOR_NORMA_SUSCRITA(ID_NORMA_SUSCRITA, ID_TIPO_FISCALIZADOR, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@IDNORMASUSCRITA, @IDTIPOFISCALIZADOR, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";
			DynamicParameters param = new();
			param.Add("IDNORMASUSCRITA", item.IdNormaSuscrita);
			param.Add("IDTIPOFISCALIZADOR", item.IdTipoFiscalizador);
			param.Add("FECHACREACION", item.FechaCreacion);
			param.Add("FECHAELIMINACION", item.FechaEliminacion);
			param.Add("VIGENCIA", item.Vigencia);

			if (transaction?.Connection != null) {
				return await transaction!.Connection!.ExecuteScalarAsync<long>(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				return await connection.ExecuteScalarAsync<long>(query, param);
			}
		}

		public async Task Actualizar(FiscalizadorNormaSuscrita item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.FISCALIZADOR_NORMA_SUSCRITA SET ID_NORMA_SUSCRITA = @IDNORMASUSCRITA, ID_TIPO_FISCALIZADOR = @IDTIPOFISCALIZADOR, FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("IDNORMASUSCRITA", item.IdNormaSuscrita);
			param.Add("IDTIPOFISCALIZADOR", item.IdTipoFiscalizador);
			param.Add("FECHACREACION", item.FechaCreacion);
			param.Add("FECHAELIMINACION", item.FechaEliminacion);
			param.Add("VIGENCIA", item.Vigencia);
			param.Add("ID", item.Id);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
