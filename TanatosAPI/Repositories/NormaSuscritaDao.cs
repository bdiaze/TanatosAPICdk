using Dapper;
using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class NormaSuscritaDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<NormaSuscrita>> ObtenerPorSub(string sub, long? idNegocio = null, bool? vigencia = true) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<NormaSuscrita>(
				"SELECT ID, SUB, ID_NEGOCIO, ID_TEMPLATE, ID_NORMA, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA, ORDEN_VISUAL, " +
				"EDITABLE, FECHA_ACTIVACION, FECHA_DESACTIVACION, ACTIVADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.NORMA_SUSCRITA " +
				"WHERE SUB = @SUB AND (ID_NEGOCIO = @IDNEGOCIO OR @IDNEGOCIO IS NULL) AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)",
				new { sub, idNegocio, vigencia }
			)];
		}

		public async Task<NormaSuscrita?> ObtenerPorId(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_NEGOCIO, ID_TEMPLATE, ID_NORMA, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA, ORDEN_VISUAL, " +
				"EDITABLE, FECHA_ACTIVACION, FECHA_DESACTIVACION, ACTIVADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.NORMA_SUSCRITA " +
				"WHERE ID = @IDNORMASUSCRITA";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("IDNORMASUSCRITA", idNormaSuscrita);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				NormaSuscrita? retorno = null;

				if (await reader.ReadAsync()) {
					retorno = new NormaSuscrita {
						Id = reader.GetInt64(0),
						Sub = reader.GetString(1),
						IdNegocio = reader.GetInt64(2),
						IdTemplate = reader.IsDBNull(3) ? null : reader.GetInt64(3),
						IdNorma = reader.IsDBNull(4) ? null : reader.GetInt64(4),
						Nombre = reader.IsDBNull(5) ? null : reader.GetString(5),
						Descripcion = reader.IsDBNull(6) ? null : reader.GetString(6),
						IdTipoPeriodicidad = reader.IsDBNull(7) ? null : reader.GetInt64(7),
						Multa = reader.IsDBNull(8) ? null : reader.GetString(8),
						IdCategoriaNorma = reader.IsDBNull(9) ? null : reader.GetInt64(9),
						OrdenVisual = reader.IsDBNull(10) ? null : reader.GetInt64(10),
						Editable = reader.GetBoolean(11),
						FechaActivacion = reader.IsDBNull(12) ? null : reader.GetDateTime(12),
						FechaDesactivacion = reader.IsDBNull(13) ? null : reader.GetDateTime(13),
						Activado = reader.GetBoolean(14),
						FechaCreacion = reader.IsDBNull(15) ? null : reader.GetDateTime(15),
						FechaEliminacion = reader.IsDBNull(16) ? null : reader.GetDateTime(16),
						Vigencia = reader.GetBoolean(17)
					};
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<long> Insertar(NormaSuscrita item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.NORMA_SUSCRITA(SUB, ID_NEGOCIO, ID_TEMPLATE, ID_NORMA, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA, ORDEN_VISUAL, EDITABLE, FECHA_ACTIVACION, FECHA_DESACTIVACION, ACTIVADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @IDNEGOCIO, @IDTEMPLATE, @IDNORMA, @NOMBRE, @DESCRIPCION, @IDTIPOPERIODICIDAD, @MULTA, @IDCATEGORIANORMA, @ORDENVISUAL, @EDITABLE, @FECHAACTIVACION, @FECHADESACTIVACION, @ACTIVADO, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";
			DynamicParameters param = new();
			param.Add("SUB", item.Sub);
			param.Add("IDNEGOCIO", item.IdNegocio);
			param.Add("IDTEMPLATE", item.IdTemplate);
			param.Add("IDNORMA", item.IdNorma);
			param.Add("NOMBRE", item.Nombre);
			param.Add("DESCRIPCION", item.Descripcion);
			param.Add("IDTIPOPERIODICIDAD", item.IdTipoPeriodicidad);
			param.Add("MULTA", item.Multa);
			param.Add("IDCATEGORIANORMA", item.IdCategoriaNorma);
			param.Add("ORDENVISUAL", item.OrdenVisual);
			param.Add("EDITABLE", item.Editable);
			param.Add("FECHAACTIVACION", item.FechaActivacion);
			param.Add("FECHADESACTIVACION", item.FechaDesactivacion);
			param.Add("ACTIVADO", item.Activado);
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

		public async Task Actualizar(NormaSuscrita item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.NORMA_SUSCRITA SET SUB = @SUB, ID_NEGOCIO = @IDNEGOCIO, ID_TEMPLATE = @IDTEMPLATE, ID_NORMA = @IDNORMA, NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, " +
				"ID_TIPO_PERIODICIDAD = @IDTIPOPERIODICIDAD, MULTA = @MULTA, ID_CATEGORIA_NORMA = @IDCATEGORIANORMA, ORDEN_VISUAL = @ORDENVISUAL, EDITABLE = @EDITABLE, " +
				"FECHA_ACTIVACION = @FECHAACTIVACION, FECHA_DESACTIVACION = @FECHADESACTIVACION, ACTIVADO = @ACTIVADO, FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("SUB", item.Sub);
			param.Add("IDNEGOCIO", item.IdNegocio);
			param.Add("IDTEMPLATE", item.IdTemplate);
			param.Add("IDNORMA", item.IdNorma);
			param.Add("NOMBRE", item.Nombre);
			param.Add("DESCRIPCION", item.Descripcion);
			param.Add("IDTIPOPERIODICIDAD", item.IdTipoPeriodicidad);
			param.Add("MULTA", item.Multa);
			param.Add("IDCATEGORIANORMA", item.IdCategoriaNorma);
			param.Add("ORDENVISUAL", item.OrdenVisual);
			param.Add("EDITABLE", item.Editable);
			param.Add("FECHAACTIVACION", item.FechaActivacion);
			param.Add("FECHADESACTIVACION", item.FechaDesactivacion);
			param.Add("ACTIVADO", item.Activado);
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
