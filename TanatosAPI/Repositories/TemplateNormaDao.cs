using Dapper;
using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TemplateNormaDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<TemplateNorma>> ObtenerPorTemplate(long idTemplate, NpgsqlTransaction? transaction = null) {
			string query = 
				"SELECT ID_TEMPLATE, ID_NORMA, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA FROM TANATOS.TEMPLATE_NORMA " +
				"WHERE ID_TEMPLATE = @IDTEMPLATE";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDTEMPLATE", idTemplate);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<TemplateNorma> retorno = [];

				while (await reader.ReadAsync()) {
					retorno.Add(new TemplateNorma {
						IdTemplate = reader.GetInt64(0),
						IdNorma = reader.GetInt64(1),
						Nombre = reader.GetString(2),
						Descripcion = reader.IsDBNull(3) ? null : reader.GetString(3),
						IdTipoPeriodicidad = reader.IsDBNull(4) ? null : reader.GetInt64(4),
						Multa = reader.IsDBNull(5) ? null : reader.GetString(5),
						IdCategoriaNorma = reader.GetInt64(6)
					});
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task Insertar(TemplateNorma item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.TEMPLATE_NORMA(ID_TEMPLATE, ID_NORMA, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA) " +
				"VALUES (@IDTEMPLATE, @IDNORMA, @NOMBRE, @DESCRIPCION, @IDTIPOPERIODICIDAD, @MULTA, @IDCATEGORIANORMA)";
			DynamicParameters param = new();
			param.Add("IDTEMPLATE", item.IdTemplate);
			param.Add("IDNORMA", item.IdNorma);
			param.Add("NOMBRE", item.Nombre);
			param.Add("DESCRIPCION", item.Descripcion);
			param.Add("IDTIPOPERIODICIDAD", item.IdTipoPeriodicidad);
			param.Add("MULTA", item.Multa);
			param.Add("IDCATEGORIANORMA", item.IdCategoriaNorma);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Actualizar(TemplateNorma item, NpgsqlTransaction? transaction = null) {
			string query = 
				"UPDATE TANATOS.TEMPLATE_NORMA SET NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, ID_TIPO_PERIODICIDAD = @IDTIPOPERIODICIDAD, " +
				"MULTA = @MULTA, ID_CATEGORIA_NORMA = @IDCATEGORIANORMA WHERE ID_TEMPLATE = @IDTEMPLATE AND ID_NORMA = @IDNORMA";
			DynamicParameters param = new();
			param.Add("NOMBRE", item.Nombre);
			param.Add("DESCRIPCION", item.Descripcion);
			param.Add("IDTIPOPERIODICIDAD", item.IdTipoPeriodicidad);
			param.Add("MULTA", item.Multa);
			param.Add("IDCATEGORIANORMA", item.IdCategoriaNorma);
			param.Add("IDTEMPLATE", item.IdTemplate);
			param.Add("IDNORMA", item.IdNorma);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Eliminar(long idTemplate, long? idNorma, NpgsqlTransaction? transaction = null) {
			string query = "DELETE FROM TANATOS.TEMPLATE_NORMA WHERE ID_TEMPLATE = @IDTEMPLATE AND (ID_NORMA = @IDNORMA OR @IDNORMA IS NULL)";
			DynamicParameters param = new();
			param.Add("IDTEMPLATE", idTemplate);
			param.Add("IDNORMA", idNorma);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
