using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class NormaSuscritaDao(IDatabaseConnectionHelper connectionHelper) {
		public async Task<List<NormaSuscrita>> ObtenerPorSub(string sub, long? idNegocio = null, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_NEGOCIO, ID_TEMPLATE, ID_NORMA, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA, ID_CARGO, ORDEN_VISUAL, " +
				"EDITABLE, FECHA_ACTIVACION, FECHA_DESACTIVACION, ACTIVADO, PROCESOS_NOTIFICACIONES, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.NORMA_SUSCRITA " +
				"WHERE SUB = @SUB AND (ID_NEGOCIO = @IDNEGOCIO OR @IDNEGOCIO IS NULL) AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("SUB", sub);
				command.Parameters.AddWithValue("IDNEGOCIO", (object?)idNegocio ?? DBNull.Value);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<NormaSuscrita> retorno = [];

				while (await reader.ReadAsync()) {
					retorno.Add(new NormaSuscrita {
						Id = reader.GetInt64(0),
						Sub = reader.GetString(1),
						IdNegocio = reader.GetInt64(2),
						IdTemplate = await reader.IsDBNullAsync(3) ? null : reader.GetInt64(3),
						IdNorma = await reader.IsDBNullAsync(4) ? null : reader.GetInt64(4),
						Nombre = await reader.IsDBNullAsync(5) ? null : reader.GetString(5),
						Descripcion = await reader.IsDBNullAsync(6) ? null : reader.GetString(6),
						IdTipoPeriodicidad = await reader.IsDBNullAsync(7) ? null : reader.GetInt64(7),
						Multa = await reader.IsDBNullAsync(8) ? null : reader.GetString(8),
						IdCategoriaNorma = await reader.IsDBNullAsync(9) ? null : reader.GetInt64(9),
                        IdCargo = await reader.IsDBNullAsync(10) ? null : reader.GetInt64(10),
                        OrdenVisual = await reader.IsDBNullAsync(11) ? null : reader.GetInt64(11),
						Editable = reader.GetBoolean(12),
						FechaActivacion = await reader.IsDBNullAsync(13) ? null : reader.GetDateTime(13),
						FechaDesactivacion = await reader.IsDBNullAsync(14) ? null : reader.GetDateTime(14),
						Activado = reader.GetBoolean(15),
						ProcesosNotificaciones = await reader.IsDBNullAsync(16) ? null : JsonSerializer.Deserialize(reader.GetString(16), AppJsonSerializerContext.Default.ListDictionaryStringJsonElement),
						FechaCreacion = await reader.IsDBNullAsync(17) ? null : reader.GetDateTime(17),
						FechaEliminacion = await reader.IsDBNullAsync(18) ? null : reader.GetDateTime(18),
						Vigencia = reader.GetBoolean(19)
					});
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<NormaSuscrita?> ObtenerPorId(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_NEGOCIO, ID_TEMPLATE, ID_NORMA, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA, ID_CARGO, ORDEN_VISUAL, " +
				"EDITABLE, FECHA_ACTIVACION, FECHA_DESACTIVACION, ACTIVADO, PROCESOS_NOTIFICACIONES, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.NORMA_SUSCRITA " +
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
						IdTemplate = await reader.IsDBNullAsync(3) ? null : reader.GetInt64(3),
						IdNorma = await reader.IsDBNullAsync(4) ? null : reader.GetInt64(4),
						Nombre = await reader.IsDBNullAsync(5) ? null : reader.GetString(5),
						Descripcion = await reader.IsDBNullAsync(6) ? null : reader.GetString(6),
						IdTipoPeriodicidad = await reader.IsDBNullAsync(7) ? null : reader.GetInt64(7),
						Multa = await reader.IsDBNullAsync(8) ? null : reader.GetString(8),
						IdCategoriaNorma = await reader.IsDBNullAsync(9) ? null : reader.GetInt64(9),
						IdCargo = await reader.IsDBNullAsync(10) ? null : reader.GetInt64(10),
                        OrdenVisual = await reader.IsDBNullAsync(11) ? null : reader.GetInt64(11),
						Editable = reader.GetBoolean(12),
						FechaActivacion = await reader.IsDBNullAsync(13) ? null : reader.GetDateTime(13),
						FechaDesactivacion = await reader.IsDBNullAsync(14) ? null : reader.GetDateTime(14),
						Activado = reader.GetBoolean(15),
						ProcesosNotificaciones = await reader.IsDBNullAsync(16) ? null : JsonSerializer.Deserialize(reader.GetString(16), AppJsonSerializerContext.Default.ListDictionaryStringJsonElement),
						FechaCreacion = await reader.IsDBNullAsync(17) ? null : reader.GetDateTime(17),
						FechaEliminacion = await reader.IsDBNullAsync(18) ? null : reader.GetDateTime(18),
						Vigencia = reader.GetBoolean(19)
					};
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

        public async Task<List<NormaSuscrita>> ObtenerPorTemplate(long idTemplate, long? idNorma = null, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, SUB, ID_NEGOCIO, ID_TEMPLATE, ID_NORMA, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA, ID_CARGO, ORDEN_VISUAL, " +
                "EDITABLE, FECHA_ACTIVACION, FECHA_DESACTIVACION, ACTIVADO, PROCESOS_NOTIFICACIONES, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.NORMA_SUSCRITA " +
                "WHERE ID_TEMPLATE = @IDTEMPLATE AND (ID_NORMA = @IDNORMA OR @IDNORMA IS NULL) AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);

                command.Parameters.AddWithValue("IDTEMPLATE", idTemplate);
                command.Parameters.AddWithValue("IDNORMA", (object?)idNorma ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                List<NormaSuscrita> retorno = [];

                while (await reader.ReadAsync()) {
                    retorno.Add(new NormaSuscrita {
                        Id = reader.GetInt64(0),
                        Sub = reader.GetString(1),
                        IdNegocio = reader.GetInt64(2),
                        IdTemplate = await reader.IsDBNullAsync(3) ? null : reader.GetInt64(3),
                        IdNorma = await reader.IsDBNullAsync(4) ? null : reader.GetInt64(4),
                        Nombre = await reader.IsDBNullAsync(5) ? null : reader.GetString(5),
                        Descripcion = await reader.IsDBNullAsync(6) ? null : reader.GetString(6),
                        IdTipoPeriodicidad = await reader.IsDBNullAsync(7) ? null : reader.GetInt64(7),
                        Multa = await reader.IsDBNullAsync(8) ? null : reader.GetString(8),
                        IdCategoriaNorma = await reader.IsDBNullAsync(9) ? null : reader.GetInt64(9),
                        IdCargo = await reader.IsDBNullAsync(10) ? null : reader.GetInt64(10),
                        OrdenVisual = await reader.IsDBNullAsync(11) ? null : reader.GetInt64(11),
                        Editable = reader.GetBoolean(12),
                        FechaActivacion = await reader.IsDBNullAsync(13) ? null : reader.GetDateTime(13),
                        FechaDesactivacion = await reader.IsDBNullAsync(14) ? null : reader.GetDateTime(14),
                        Activado = reader.GetBoolean(15),
                        ProcesosNotificaciones = await reader.IsDBNullAsync(16) ? null : JsonSerializer.Deserialize(reader.GetString(16), AppJsonSerializerContext.Default.ListDictionaryStringJsonElement),
                        FechaCreacion = await reader.IsDBNullAsync(17) ? null : reader.GetDateTime(17),
                        FechaEliminacion = await reader.IsDBNullAsync(18) ? null : reader.GetDateTime(18),
                        Vigencia = reader.GetBoolean(19)
                    });
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
				"INSERT INTO TANATOS.NORMA_SUSCRITA(SUB, ID_NEGOCIO, ID_TEMPLATE, ID_NORMA, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA, ID_CARGO, ORDEN_VISUAL, EDITABLE, FECHA_ACTIVACION, FECHA_DESACTIVACION, ACTIVADO, PROCESOS_NOTIFICACIONES, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
                "VALUES (@SUB, @IDNEGOCIO, @IDTEMPLATE, @IDNORMA, @NOMBRE, @DESCRIPCION, @IDTIPOPERIODICIDAD, @MULTA, @IDCATEGORIANORMA, @IDCARGO, @ORDENVISUAL, @EDITABLE, @FECHAACTIVACION, @FECHADESACTIVACION, @ACTIVADO, @PROCESOSNOTIFICACIONES::JSONB, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("IDNEGOCIO", item.IdNegocio);
                command.Parameters.AddWithValue("IDTEMPLATE", (object?)item.IdTemplate ?? DBNull.Value);
                command.Parameters.AddWithValue("IDNORMA", (object?)item.IdNorma ?? DBNull.Value);
                command.Parameters.AddWithValue("NOMBRE", (object?)item.Nombre ?? DBNull.Value);
                command.Parameters.AddWithValue("DESCRIPCION", (object?)item.Descripcion ?? DBNull.Value);
                command.Parameters.AddWithValue("IDTIPOPERIODICIDAD", (object?)item.IdTipoPeriodicidad ?? DBNull.Value);
                command.Parameters.AddWithValue("MULTA", (object?)item.Multa ?? DBNull.Value);
                command.Parameters.AddWithValue("IDCATEGORIANORMA", (object?)item.IdCategoriaNorma ?? DBNull.Value);
                command.Parameters.AddWithValue("IDCARGO", (object?)item.IdCargo ?? DBNull.Value);
                command.Parameters.AddWithValue("ORDENVISUAL", (object?)item.OrdenVisual ?? DBNull.Value);
                command.Parameters.AddWithValue("EDITABLE", item.Editable);
                command.Parameters.AddWithValue("FECHAACTIVACION", (object?)item.FechaActivacion ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHADESACTIVACION", (object?)item.FechaDesactivacion ?? DBNull.Value);
                command.Parameters.AddWithValue("ACTIVADO", item.Activado);
                command.Parameters.AddWithValue("PROCESOSNOTIFICACIONES", item.ProcesosNotificaciones == null ? DBNull.Value : JsonSerializer.Serialize(item.ProcesosNotificaciones, AppJsonSerializerContext.Default.ListDictionaryStringJsonElement));
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

		public async Task Actualizar(NormaSuscrita item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.NORMA_SUSCRITA SET SUB = @SUB, ID_NEGOCIO = @IDNEGOCIO, ID_TEMPLATE = @IDTEMPLATE, " +
                "ID_NORMA = @IDNORMA, NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, " +
                "ID_TIPO_PERIODICIDAD = @IDTIPOPERIODICIDAD, MULTA = @MULTA, ID_CATEGORIA_NORMA = @IDCATEGORIANORMA, " +
                "ID_CARGO = @IDCARGO, ORDEN_VISUAL = @ORDENVISUAL, EDITABLE = @EDITABLE, " +
				"FECHA_ACTIVACION = @FECHAACTIVACION, FECHA_DESACTIVACION = @FECHADESACTIVACION, ACTIVADO = @ACTIVADO, " +
                "PROCESOS_NOTIFICACIONES = @PROCESOSNOTIFICACIONES::JSONB, FECHA_CREACION = @FECHACREACION, " +
                "FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("IDNEGOCIO", item.IdNegocio);
                command.Parameters.AddWithValue("IDTEMPLATE", (object?)item.IdTemplate ?? DBNull.Value);
                command.Parameters.AddWithValue("IDNORMA", (object?)item.IdNorma ?? DBNull.Value);
                command.Parameters.AddWithValue("NOMBRE", (object?)item.Nombre ?? DBNull.Value);
                command.Parameters.AddWithValue("DESCRIPCION", (object?)item.Descripcion ?? DBNull.Value);
                command.Parameters.AddWithValue("IDTIPOPERIODICIDAD", (object?)item.IdTipoPeriodicidad ?? DBNull.Value);
                command.Parameters.AddWithValue("MULTA", (object?)item.Multa ?? DBNull.Value);
                command.Parameters.AddWithValue("IDCATEGORIANORMA", (object?)item.IdCategoriaNorma ?? DBNull.Value);
                command.Parameters.AddWithValue("IDCARGO", (object?)item.IdCargo ?? DBNull.Value);
                command.Parameters.AddWithValue("ORDENVISUAL", (object?)item.OrdenVisual ?? DBNull.Value);
                command.Parameters.AddWithValue("EDITABLE", item.Editable);
                command.Parameters.AddWithValue("FECHAACTIVACION", (object?)item.FechaActivacion ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHADESACTIVACION", (object?)item.FechaDesactivacion ?? DBNull.Value);
                command.Parameters.AddWithValue("ACTIVADO", item.Activado);
                command.Parameters.AddWithValue("PROCESOSNOTIFICACIONES", item.ProcesosNotificaciones == null ? DBNull.Value : JsonSerializer.Serialize(item.ProcesosNotificaciones, AppJsonSerializerContext.Default.ListDictionaryStringJsonElement));
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
