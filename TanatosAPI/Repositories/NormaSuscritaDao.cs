using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class NormaSuscritaDao(IDatabaseConnectionHelper connectionHelper) : INormaSuscritaDao {
		public async Task<List<NormaSuscrita>> ObtenerPorSub(string sub, long? idNegocio = null, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_NEGOCIO, ID_TEMPLATE, ID_NORMA, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA, ID_CARGO, ORDEN_VISUAL, " +
				"EDITABLE, FECHA_ACTIVACION, FECHA_DESACTIVACION, ACTIVADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.NORMA_SUSCRITA " +
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
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						IdNegocio = reader.GetInt64(reader.GetOrdinal("ID_NEGOCIO")),
						IdTemplate = await reader.IsDBNullAsync(reader.GetOrdinal("ID_TEMPLATE")) ? null : reader.GetInt64(reader.GetOrdinal("ID_TEMPLATE")),
						IdNorma = await reader.IsDBNullAsync(reader.GetOrdinal("ID_NORMA")) ? null : reader.GetInt64(reader.GetOrdinal("ID_NORMA")),
						Nombre = await reader.IsDBNullAsync(reader.GetOrdinal("NOMBRE")) ? null : reader.GetString(reader.GetOrdinal("NOMBRE")),
						Descripcion = await reader.IsDBNullAsync(reader.GetOrdinal("DESCRIPCION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPCION")),
						IdTipoPeriodicidad = await reader.IsDBNullAsync(reader.GetOrdinal("ID_TIPO_PERIODICIDAD")) ? null : reader.GetInt64(reader.GetOrdinal("ID_TIPO_PERIODICIDAD")),
						Multa = await reader.IsDBNullAsync(reader.GetOrdinal("MULTA")) ? null : reader.GetString(reader.GetOrdinal("MULTA")),
						IdCategoriaNorma = await reader.IsDBNullAsync(reader.GetOrdinal("ID_CATEGORIA_NORMA")) ? null : reader.GetInt64(reader.GetOrdinal("ID_CATEGORIA_NORMA")),
                        IdCargo = await reader.IsDBNullAsync(reader.GetOrdinal("ID_CARGO")) ? null : reader.GetInt64(reader.GetOrdinal("ID_CARGO")),
                        OrdenVisual = await reader.IsDBNullAsync(reader.GetOrdinal("ORDEN_VISUAL")) ? null : reader.GetInt64(reader.GetOrdinal("ORDEN_VISUAL")),
						Editable = reader.GetBoolean(reader.GetOrdinal("EDITABLE")),
						FechaActivacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_ACTIVACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_ACTIVACION")),
						FechaDesactivacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_DESACTIVACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_DESACTIVACION")),
						Activado = reader.GetBoolean(reader.GetOrdinal("ACTIVADO")),
						FechaCreacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_CREACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
						FechaEliminacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_ELIMINACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_ELIMINACION")),
						Vigencia = reader.GetBoolean(reader.GetOrdinal("VIGENCIA"))
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
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						IdNegocio = reader.GetInt64(reader.GetOrdinal("ID_NEGOCIO")),
						IdTemplate = await reader.IsDBNullAsync(reader.GetOrdinal("ID_TEMPLATE")) ? null : reader.GetInt64(reader.GetOrdinal("ID_TEMPLATE")),
						IdNorma = await reader.IsDBNullAsync(reader.GetOrdinal("ID_NORMA")) ? null : reader.GetInt64(reader.GetOrdinal("ID_NORMA")),
						Nombre = await reader.IsDBNullAsync(reader.GetOrdinal("NOMBRE")) ? null : reader.GetString(reader.GetOrdinal("NOMBRE")),
						Descripcion = await reader.IsDBNullAsync(reader.GetOrdinal("DESCRIPCION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPCION")),
						IdTipoPeriodicidad = await reader.IsDBNullAsync(reader.GetOrdinal("ID_TIPO_PERIODICIDAD")) ? null : reader.GetInt64(reader.GetOrdinal("ID_TIPO_PERIODICIDAD")),
						Multa = await reader.IsDBNullAsync(reader.GetOrdinal("MULTA")) ? null : reader.GetString(reader.GetOrdinal("MULTA")),
						IdCategoriaNorma = await reader.IsDBNullAsync(reader.GetOrdinal("ID_CATEGORIA_NORMA")) ? null : reader.GetInt64(reader.GetOrdinal("ID_CATEGORIA_NORMA")),
						IdCargo = await reader.IsDBNullAsync(reader.GetOrdinal("ID_CARGO")) ? null : reader.GetInt64(reader.GetOrdinal("ID_CARGO")),
						OrdenVisual = await reader.IsDBNullAsync(reader.GetOrdinal("ORDEN_VISUAL")) ? null : reader.GetInt64(reader.GetOrdinal("ORDEN_VISUAL")),
						Editable = reader.GetBoolean(reader.GetOrdinal("EDITABLE")),
						FechaActivacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_ACTIVACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_ACTIVACION")),
						FechaDesactivacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_DESACTIVACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_DESACTIVACION")),
						Activado = reader.GetBoolean(reader.GetOrdinal("ACTIVADO")),
						FechaCreacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_CREACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
						FechaEliminacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_ELIMINACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_ELIMINACION")),
						Vigencia = reader.GetBoolean(reader.GetOrdinal("VIGENCIA"))
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
                "EDITABLE, FECHA_ACTIVACION, FECHA_DESACTIVACION, ACTIVADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.NORMA_SUSCRITA " +
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
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						IdNegocio = reader.GetInt64(reader.GetOrdinal("ID_NEGOCIO")),
						IdTemplate = await reader.IsDBNullAsync(reader.GetOrdinal("ID_TEMPLATE")) ? null : reader.GetInt64(reader.GetOrdinal("ID_TEMPLATE")),
						IdNorma = await reader.IsDBNullAsync(reader.GetOrdinal("ID_NORMA")) ? null : reader.GetInt64(reader.GetOrdinal("ID_NORMA")),
						Nombre = await reader.IsDBNullAsync(reader.GetOrdinal("NOMBRE")) ? null : reader.GetString(reader.GetOrdinal("NOMBRE")),
						Descripcion = await reader.IsDBNullAsync(reader.GetOrdinal("DESCRIPCION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPCION")),
						IdTipoPeriodicidad = await reader.IsDBNullAsync(reader.GetOrdinal("ID_TIPO_PERIODICIDAD")) ? null : reader.GetInt64(reader.GetOrdinal("ID_TIPO_PERIODICIDAD")),
						Multa = await reader.IsDBNullAsync(reader.GetOrdinal("MULTA")) ? null : reader.GetString(reader.GetOrdinal("MULTA")),
						IdCategoriaNorma = await reader.IsDBNullAsync(reader.GetOrdinal("ID_CATEGORIA_NORMA")) ? null : reader.GetInt64(reader.GetOrdinal("ID_CATEGORIA_NORMA")),
						IdCargo = await reader.IsDBNullAsync(reader.GetOrdinal("ID_CARGO")) ? null : reader.GetInt64(reader.GetOrdinal("ID_CARGO")),
						OrdenVisual = await reader.IsDBNullAsync(reader.GetOrdinal("ORDEN_VISUAL")) ? null : reader.GetInt64(reader.GetOrdinal("ORDEN_VISUAL")),
						Editable = reader.GetBoolean(reader.GetOrdinal("EDITABLE")),
						FechaActivacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_ACTIVACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_ACTIVACION")),
						FechaDesactivacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_DESACTIVACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_DESACTIVACION")),
						Activado = reader.GetBoolean(reader.GetOrdinal("ACTIVADO")),
						FechaCreacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_CREACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
						FechaEliminacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_ELIMINACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_ELIMINACION")),
						Vigencia = reader.GetBoolean(reader.GetOrdinal("VIGENCIA"))
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
				"INSERT INTO TANATOS.NORMA_SUSCRITA(SUB, ID_NEGOCIO, ID_TEMPLATE, ID_NORMA, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA, ID_CARGO, ORDEN_VISUAL, EDITABLE, FECHA_ACTIVACION, FECHA_DESACTIVACION, ACTIVADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
                "VALUES (@SUB, @IDNEGOCIO, @IDTEMPLATE, @IDNORMA, @NOMBRE, @DESCRIPCION, @IDTIPOPERIODICIDAD, @MULTA, @IDCATEGORIANORMA, @IDCARGO, @ORDENVISUAL, @EDITABLE, @FECHAACTIVACION, @FECHADESACTIVACION, @ACTIVADO, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
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
                "FECHA_CREACION = @FECHACREACION, " +
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
