using Dapper;
using Npgsql;
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

		public async Task<long> Insertar(NormaSuscrita item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.ExecuteScalarAsync<long>(
				"INSERT INTO TANATOS.NORMA_SUSCRITA(SUB, ID_NEGOCIO, ID_TEMPLATE, ID_NORMA, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA, ORDEN_VISUAL, EDITABLE, FECHA_ACTIVACION, FECHA_DESACTIVACION, ACTIVADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @IDNEGOCIO, @IDTEMPLATE, @IDNORMA, @NOMBRE, @DESCRIPCION, @IDTIPOPERIODICIDAD, @MULTA, @IDCATEGORIANORMA, @ORDENVISUAL, @EDITABLE, @FECHAACTIVACION, @FECHADESACTIVACION, @ACTIVADO, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID",
				new { item.Sub, item.IdNegocio, item.IdTemplate, item.IdNorma, item.Nombre, item.Descripcion, item.IdTipoPeriodicidad, item.Multa, item.IdCategoriaNorma, item.OrdenVisual, item.Editable, item.FechaActivacion, item.FechaDesactivacion, item.Activado, item.FechaCreacion, item.FechaEliminacion, item.Vigencia }
			);
		}

		public async Task Actualizar(NormaSuscrita item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.NORMA_SUSCRITA SET SUB = @SUB, ID_NEGOCIO = @IDNEGOCIO, ID_TEMPLATE = @IDTEMPLATE, ID_NORMA = @IDNORMA, NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, " +
				"ID_TIPO_PERIODICIDAD = @IDTIPOPERIODICIDAD, MULTA = @MULTA, ID_CATEGORIA_NORMA = @IDCATEGORIANORMA, ORDEN_VISUAL = @ORDENVISUAL, EDITABLE = @EDITABLE, " +
				"FECHA_ACTIVACION = @FECHAACTIVACION, FECHA_DESACTIVACION = @FECHADESACTIVACION, ACTIVADO = @ACTIVADO, FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID",
				new { item.Sub, item.IdNegocio, item.IdTemplate, item.IdNorma, item.Nombre, item.Descripcion, item.IdTipoPeriodicidad, item.Multa, item.IdCategoriaNorma, item.OrdenVisual, item.Editable, item.FechaActivacion, item.FechaDesactivacion, item.Activado, item.FechaCreacion, item.FechaEliminacion, item.Vigencia, item.Id }
			);
		}
	}
}
