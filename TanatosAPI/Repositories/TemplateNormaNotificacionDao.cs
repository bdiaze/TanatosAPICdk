using Dapper;
using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TemplateNormaNotificacionDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<TemplateNormaNotificacion>> ObtenerPorTemplateNorma(long idTemplate, long? idNorma = null, NpgsqlTransaction? transaction = null) {
			string query = 
				"SELECT ID_TEMPLATE, ID_NORMA, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION FROM TANATOS.TEMPLATE_NORMA_NOTIFICACION " +
				"WHERE ID_TEMPLATE = @IDTEMPLATE AND (ID_NORMA = @IDNORMA OR @IDNORMA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("IDTEMPLATE", idTemplate);
				command.Parameters.AddWithValue("IDNORMA", (object?)idNorma ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<TemplateNormaNotificacion> retorno = [];

				while (await reader.ReadAsync()) {
					retorno.Add(new TemplateNormaNotificacion {
						IdTemplate = reader.GetInt64(0),
						IdNorma = reader.GetInt64(1),
						IdTipoUnidadTiempoAntelacion = reader.GetInt64(2),
						CantAntelacion = reader.GetInt32(3),
					});
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<List<TemplateNormaNotificacion>> ObtenerPorTipoUnidadTiempoAntelacion(long idTipoUnidadTiempoAntelacion) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TemplateNormaNotificacion>(
				"SELECT ID_TEMPLATE, ID_NORMA, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION FROM TANATOS.TEMPLATE_NORMA_NOTIFICACION WHERE ID_TIPO_UNIDAD_TIEMPO_ANTELACION = @IDTIPOUNIDADTIEMPOANTELACION",
				new { idTipoUnidadTiempoAntelacion }
			)];
		}

		public async Task Insertar(TemplateNormaNotificacion item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.TEMPLATE_NORMA_NOTIFICACION(ID_TEMPLATE, ID_NORMA, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION) " +
				"VALUES (@IDTEMPLATE, @IDNORMA, @IDTIPOUNIDADTIEMPOANTELACION, @CANTANTELACION)";
			DynamicParameters param = new();
			param.Add("IDTEMPLATE", item.IdTemplate);
			param.Add("IDNORMA", item.IdNorma);
			param.Add("IDTIPOUNIDADTIEMPOANTELACION", item.IdTipoUnidadTiempoAntelacion);
			param.Add("CANTANTELACION", item.CantAntelacion);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Eliminar(long idTemplate, long? idNorma, long? idTipoUnidadTiempoAntelacion, int? cantAntelacion, NpgsqlTransaction? transaction = null) {
			string query = "DELETE FROM TANATOS.TEMPLATE_NORMA_NOTIFICACION WHERE ID_TEMPLATE = @IDTEMPLATE AND (ID_NORMA = @IDNORMA OR @IDNORMA IS NULL) AND (ID_TIPO_UNIDAD_TIEMPO_ANTELACION = @IDTIPOUNIDADTIEMPOANTELACION OR @IDTIPOUNIDADTIEMPOANTELACION IS NULL) AND (CANT_ANTELACION = @CANT_ANTELACION OR @CANT_ANTELACION IS NULL)";
			DynamicParameters param = new();
			param.Add("IDTEMPLATE", idTemplate);
			param.Add("IDNORMA", idNorma);
			param.Add("IDTIPOUNIDADTIEMPOANTELACION", idTipoUnidadTiempoAntelacion);
			param.Add("CANT_ANTELACION", cantAntelacion);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
