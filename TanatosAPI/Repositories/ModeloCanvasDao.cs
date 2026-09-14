using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	public class ModeloCanvasDao(IDatabaseConnectionHelper connectionHelper) : IModeloCanvasDao {
		public async Task<ModeloCanvas?> Obtener(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_NEGOCIO, SOCIOS_CLAVE, ACTIVIDADES_CLAVE, RECURSOS_CLAVE, PROPUESTA_VALOR, RELACIONES_CLIENTES, " +
				"CANALES, SEGMENTOS_CLIENTES, ESTRUCTURA_COSTOS, FUENTES_INGRESOS, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.MODELO_CANVAS " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				ModeloCanvas? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new ModeloCanvas {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						IdNegocio = reader.GetInt64(reader.GetOrdinal("ID_NEGOCIO")),
						SociosClave = await reader.IsDBNullAsync(reader.GetOrdinal("SOCIOS_CLAVE")) ? null : reader.GetString(reader.GetOrdinal("SOCIOS_CLAVE")),
						ActividadesClave = await reader.IsDBNullAsync(reader.GetOrdinal("ACTIVIDADES_CLAVE")) ? null : reader.GetString(reader.GetOrdinal("ACTIVIDADES_CLAVE")),
						RecursosClave = await reader.IsDBNullAsync(reader.GetOrdinal("RECURSOS_CLAVE")) ? null : reader.GetString(reader.GetOrdinal("RECURSOS_CLAVE")),
						PropuestaValor = await reader.IsDBNullAsync(reader.GetOrdinal("PROPUESTA_VALOR")) ? null : reader.GetString(reader.GetOrdinal("PROPUESTA_VALOR")),
						RelacionesClientes = await reader.IsDBNullAsync(reader.GetOrdinal("RELACIONES_CLIENTES")) ? null : reader.GetString(reader.GetOrdinal("RELACIONES_CLIENTES")),
						Canales = await reader.IsDBNullAsync(reader.GetOrdinal("CANALES")) ? null : reader.GetString(reader.GetOrdinal("CANALES")),
						SegmentosClientes = await reader.IsDBNullAsync(reader.GetOrdinal("SEGMENTOS_CLIENTES")) ? null : reader.GetString(reader.GetOrdinal("SEGMENTOS_CLIENTES")),
						EstructuraCostos = await reader.IsDBNullAsync(reader.GetOrdinal("ESTRUCTURA_COSTOS")) ? null : reader.GetString(reader.GetOrdinal("ESTRUCTURA_COSTOS")),
						FuentesIngresos = await reader.IsDBNullAsync(reader.GetOrdinal("FUENTES_INGRESOS")) ? null : reader.GetString(reader.GetOrdinal("FUENTES_INGRESOS")),
						FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
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

		public async Task<List<ModeloCanvas>> ObtenerPorSub(string sub, long? idNegocio = null, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_NEGOCIO, SOCIOS_CLAVE, ACTIVIDADES_CLAVE, RECURSOS_CLAVE, PROPUESTA_VALOR, RELACIONES_CLIENTES, " +
				"CANALES, SEGMENTOS_CLIENTES, ESTRUCTURA_COSTOS, FUENTES_INGRESOS, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.MODELO_CANVAS " +
				"WHERE SUB = @SUB AND (ID_NEGOCIO = @IDNEGOCIO OR @IDNEGOCIO IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", sub);
				command.Parameters.AddWithValue("IDNEGOCIO", (object?)idNegocio ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<ModeloCanvas> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new ModeloCanvas {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Sub = reader.GetString(reader.GetOrdinal("SUB")),
						IdNegocio = reader.GetInt64(reader.GetOrdinal("ID_NEGOCIO")),
						SociosClave = await reader.IsDBNullAsync(reader.GetOrdinal("SOCIOS_CLAVE")) ? null : reader.GetString(reader.GetOrdinal("SOCIOS_CLAVE")),
						ActividadesClave = await reader.IsDBNullAsync(reader.GetOrdinal("ACTIVIDADES_CLAVE")) ? null : reader.GetString(reader.GetOrdinal("ACTIVIDADES_CLAVE")),
						RecursosClave = await reader.IsDBNullAsync(reader.GetOrdinal("RECURSOS_CLAVE")) ? null : reader.GetString(reader.GetOrdinal("RECURSOS_CLAVE")),
						PropuestaValor = await reader.IsDBNullAsync(reader.GetOrdinal("PROPUESTA_VALOR")) ? null : reader.GetString(reader.GetOrdinal("PROPUESTA_VALOR")),
						RelacionesClientes = await reader.IsDBNullAsync(reader.GetOrdinal("RELACIONES_CLIENTES")) ? null : reader.GetString(reader.GetOrdinal("RELACIONES_CLIENTES")),
						Canales = await reader.IsDBNullAsync(reader.GetOrdinal("CANALES")) ? null : reader.GetString(reader.GetOrdinal("CANALES")),
						SegmentosClientes = await reader.IsDBNullAsync(reader.GetOrdinal("SEGMENTOS_CLIENTES")) ? null : reader.GetString(reader.GetOrdinal("SEGMENTOS_CLIENTES")),
						EstructuraCostos = await reader.IsDBNullAsync(reader.GetOrdinal("ESTRUCTURA_COSTOS")) ? null : reader.GetString(reader.GetOrdinal("ESTRUCTURA_COSTOS")),
						FuentesIngresos = await reader.IsDBNullAsync(reader.GetOrdinal("FUENTES_INGRESOS")) ? null : reader.GetString(reader.GetOrdinal("FUENTES_INGRESOS")),
						FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
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

		public async Task<long> Insertar(ModeloCanvas item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.MODELO_CANVAS(SUB, ID_NEGOCIO, SOCIOS_CLAVE, ACTIVIDADES_CLAVE, RECURSOS_CLAVE, PROPUESTA_VALOR, RELACIONES_CLIENTES, CANALES, SEGMENTOS_CLIENTES, ESTRUCTURA_COSTOS, FUENTES_INGRESOS, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @IDNEGOCIO, @SOCIOSCLAVE, @ACTIVIDADESCLAVE, @RECURSOSCLAVE, @PROPUESTAVALOR, @RELACIONESCLIENTES, @CANALES, @SEGMENTOSCLIENTES, @ESTRUCTURACOSTOS, @FUENTESINGRESOS, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", item.Sub);
				command.Parameters.AddWithValue("IDNEGOCIO", item.IdNegocio);
				command.Parameters.AddWithValue("SOCIOSCLAVE", (object?)item.SociosClave ?? DBNull.Value);
				command.Parameters.AddWithValue("ACTIVIDADESCLAVE", (object?)item.ActividadesClave ?? DBNull.Value);
				command.Parameters.AddWithValue("RECURSOSCLAVE", (object?)item.RecursosClave ?? DBNull.Value);
				command.Parameters.AddWithValue("PROPUESTAVALOR", (object?)item.PropuestaValor ?? DBNull.Value);
				command.Parameters.AddWithValue("RELACIONESCLIENTES", (object?)item.RelacionesClientes ?? DBNull.Value);
				command.Parameters.AddWithValue("CANALES", (object?)item.Canales ?? DBNull.Value);
				command.Parameters.AddWithValue("SEGMENTOSCLIENTES", (object?)item.SegmentosClientes ?? DBNull.Value);
				command.Parameters.AddWithValue("ESTRUCTURACOSTOS", (object?)item.EstructuraCostos ?? DBNull.Value);
				command.Parameters.AddWithValue("FUENTESINGRESOS", (object?)item.FuentesIngresos ?? DBNull.Value);
				command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);
				command.Parameters.AddWithValue("FECHAELIMINACION", (object?)item.FechaEliminacion ?? DBNull.Value);
				command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
				return Convert.ToInt64(await command.ExecuteScalarAsync());
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task Actualizar(ModeloCanvas item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.MODELO_CANVAS SET SUB = @SUB, ID_NEGOCIO = @IDNEGOCIO, SOCIOS_CLAVE = @SOCIOSCLAVE, ACTIVIDADES_CLAVE = @ACTIVIDADESCLAVE, " +
				"RECURSOS_CLAVE = @RECURSOSCLAVE, PROPUESTA_VALOR = @PROPUESTAVALOR, RELACIONES_CLIENTES = @RELACIONESCLIENTES, CANALES = @CANALES, " +
				"SEGMENTOS_CLIENTES = @SEGMENTOSCLIENTES, ESTRUCTURA_COSTOS = @ESTRUCTURACOSTOS, FUENTES_INGRESOS = @FUENTESINGRESOS, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", item.Sub);
				command.Parameters.AddWithValue("IDNEGOCIO", item.IdNegocio);
				command.Parameters.AddWithValue("SOCIOSCLAVE", (object?)item.SociosClave ?? DBNull.Value);
				command.Parameters.AddWithValue("ACTIVIDADESCLAVE", (object?)item.ActividadesClave ?? DBNull.Value);
				command.Parameters.AddWithValue("RECURSOSCLAVE", (object?)item.RecursosClave ?? DBNull.Value);
				command.Parameters.AddWithValue("PROPUESTAVALOR", (object?)item.PropuestaValor ?? DBNull.Value);
				command.Parameters.AddWithValue("RELACIONESCLIENTES", (object?)item.RelacionesClientes ?? DBNull.Value);
				command.Parameters.AddWithValue("CANALES", (object?)item.Canales ?? DBNull.Value);
				command.Parameters.AddWithValue("SEGMENTOSCLIENTES", (object?)item.SegmentosClientes ?? DBNull.Value);
				command.Parameters.AddWithValue("ESTRUCTURACOSTOS", (object?)item.EstructuraCostos ?? DBNull.Value);
				command.Parameters.AddWithValue("FUENTESINGRESOS", (object?)item.FuentesIngresos ?? DBNull.Value);
				command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);
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
