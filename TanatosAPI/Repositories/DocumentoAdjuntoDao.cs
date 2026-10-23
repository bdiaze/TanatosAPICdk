using Npgsql;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class DocumentoAdjuntoDao(IDatabaseConnectionHelper connectionHelper): IDocumentoAdjuntoDao {
		public async Task<List<DocumentoAdjunto>> ObtenerPorHistorial(long idHistorialNormaSuscrita, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_HISTORIAL_NORMA_SUSCRITA, BUCKET_NAME, BUCKET_KEY, NOMBRE_ARCHIVO, MIME_ESPERADO, TAMANNO_ESPERADO, MIME_REAL, TAMANNO_REAL, " +
				"ESTADO_SUBIDA, FECHA_EMISION_URL_PREFIRMADA_PUT, FECHA_CONFIRMACION_SUBIDA, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.DOCUMENTO_ADJUNTO " +
				"WHERE ID_HISTORIAL_NORMA_SUSCRITA = @IDHISTORIALNORMASUSCRITA AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDHISTORIALNORMASUSCRITA", idHistorialNormaSuscrita);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				List<DocumentoAdjunto> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new DocumentoAdjunto {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						IdHistorialNormaSuscrita = await reader.IsDBNullAsync(reader.GetOrdinal("ID_HISTORIAL_NORMA_SUSCRITA")) ? null : reader.GetInt64(reader.GetOrdinal("ID_HISTORIAL_NORMA_SUSCRITA")),
						BucketName = reader.GetString(reader.GetOrdinal("BUCKET_NAME")),
						BucketKey = reader.GetString(reader.GetOrdinal("BUCKET_KEY")),
						NombreArchivo = reader.GetString(reader.GetOrdinal("NOMBRE_ARCHIVO")),
						MimeEsperado = reader.GetString(reader.GetOrdinal("MIME_ESPERADO")),
						TamannoEsperado = reader.GetInt64(reader.GetOrdinal("TAMANNO_ESPERADO")),
						MimeReal = await reader.IsDBNullAsync(reader.GetOrdinal("MIME_REAL")) ? null : reader.GetString(reader.GetOrdinal("MIME_REAL")),
						TamannoReal = await reader.IsDBNullAsync(reader.GetOrdinal("TAMANNO_REAL")) ? null : reader.GetInt64(reader.GetOrdinal("TAMANNO_REAL")),
						EstadoSubida = reader.GetInt16(reader.GetOrdinal("ESTADO_SUBIDA")),
						FechaEmisionUrlPrefirmadaPut = reader.GetDateTime(reader.GetOrdinal("FECHA_EMISION_URL_PREFIRMADA_PUT")),
						FechaConfirmacionSubida = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_CONFIRMACION_SUBIDA")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_CONFIRMACION_SUBIDA")),
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

		public async Task<DocumentoAdjunto?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_HISTORIAL_NORMA_SUSCRITA, BUCKET_NAME, BUCKET_KEY, NOMBRE_ARCHIVO, MIME_ESPERADO, TAMANNO_ESPERADO, MIME_REAL, TAMANNO_REAL, " +
				"ESTADO_SUBIDA, FECHA_EMISION_URL_PREFIRMADA_PUT, FECHA_CONFIRMACION_SUBIDA, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.DOCUMENTO_ADJUNTO " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				DocumentoAdjunto? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new DocumentoAdjunto {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						IdHistorialNormaSuscrita = await reader.IsDBNullAsync(reader.GetOrdinal("ID_HISTORIAL_NORMA_SUSCRITA")) ? null : reader.GetInt64(reader.GetOrdinal("ID_HISTORIAL_NORMA_SUSCRITA")),
						BucketName = reader.GetString(reader.GetOrdinal("BUCKET_NAME")),
						BucketKey = reader.GetString(reader.GetOrdinal("BUCKET_KEY")),
						NombreArchivo = reader.GetString(reader.GetOrdinal("NOMBRE_ARCHIVO")),
						MimeEsperado = reader.GetString(reader.GetOrdinal("MIME_ESPERADO")),
						TamannoEsperado = reader.GetInt64(reader.GetOrdinal("TAMANNO_ESPERADO")),
						MimeReal = await reader.IsDBNullAsync(reader.GetOrdinal("MIME_REAL")) ? null : reader.GetString(reader.GetOrdinal("MIME_REAL")),
						TamannoReal = await reader.IsDBNullAsync(reader.GetOrdinal("TAMANNO_REAL")) ? null : reader.GetInt64(reader.GetOrdinal("TAMANNO_REAL")),
						EstadoSubida = reader.GetInt16(reader.GetOrdinal("ESTADO_SUBIDA")),
						FechaEmisionUrlPrefirmadaPut = reader.GetDateTime(reader.GetOrdinal("FECHA_EMISION_URL_PREFIRMADA_PUT")),
						FechaConfirmacionSubida = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_CONFIRMACION_SUBIDA")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_CONFIRMACION_SUBIDA")),
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

		public async Task<long> Insertar(DocumentoAdjunto item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.DOCUMENTO_ADJUNTO(ID_HISTORIAL_NORMA_SUSCRITA, BUCKET_NAME, BUCKET_KEY, NOMBRE_ARCHIVO, MIME_ESPERADO, TAMANNO_ESPERADO, " +
				"MIME_REAL, TAMANNO_REAL, ESTADO_SUBIDA, FECHA_EMISION_URL_PREFIRMADA_PUT, FECHA_CONFIRMACION_SUBIDA, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@IDHISTORIALNORMASUSCRITA, @BUCKETNAME, @BUCKETKEY, @NOMBREARCHIVO, @MIMEESPERADO, @TAMANNOESPERADO, " +
				"@MIMEREAL, @TAMANNOREAL, @ESTADOSUBIDA, @FECHAEMISIONURLPREFIRMADAPUT, @FECHACONFIRMACIONSUBIDA, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDHISTORIALNORMASUSCRITA", (object?)item.IdHistorialNormaSuscrita ?? DBNull.Value);
                command.Parameters.AddWithValue("BUCKETNAME", item.BucketName);
                command.Parameters.AddWithValue("BUCKETKEY", item.BucketKey);
                command.Parameters.AddWithValue("NOMBREARCHIVO", item.NombreArchivo);
                command.Parameters.AddWithValue("MIMEESPERADO", item.MimeEsperado);
                command.Parameters.AddWithValue("TAMANNOESPERADO", item.TamannoEsperado);
                command.Parameters.AddWithValue("MIMEREAL", (object?)item.MimeReal ?? DBNull.Value);
                command.Parameters.AddWithValue("TAMANNOREAL", (object?)item.TamannoReal ?? DBNull.Value);
                command.Parameters.AddWithValue("ESTADOSUBIDA", item.EstadoSubida);
                command.Parameters.AddWithValue("FECHAEMISIONURLPREFIRMADAPUT", item.FechaEmisionUrlPrefirmadaPut);
                command.Parameters.AddWithValue("FECHACONFIRMACIONSUBIDA", (object?)item.FechaConfirmacionSubida ?? DBNull.Value);
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

		public async Task Actualizar(DocumentoAdjunto item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.DOCUMENTO_ADJUNTO SET ID_HISTORIAL_NORMA_SUSCRITA = @IDHISTORIALNORMASUSCRITA, BUCKET_NAME = @BUCKETNAME, BUCKET_KEY = @BUCKETKEY, " +
				"NOMBRE_ARCHIVO = @NOMBREARCHIVO, MIME_ESPERADO = @MIMEESPERADO, TAMANNO_ESPERADO = @TAMANNOESPERADO, MIME_REAL = @MIMEREAL, TAMANNO_REAL = @TAMANNOREAL, " +
				"ESTADO_SUBIDA = @ESTADOSUBIDA, FECHA_EMISION_URL_PREFIRMADA_PUT = @FECHAEMISIONURLPREFIRMADAPUT, FECHA_CONFIRMACION_SUBIDA = @FECHACONFIRMACIONSUBIDA, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDHISTORIALNORMASUSCRITA", (object?)item.IdHistorialNormaSuscrita ?? DBNull.Value);
                command.Parameters.AddWithValue("BUCKETNAME", item.BucketName);
                command.Parameters.AddWithValue("BUCKETKEY", item.BucketKey);
                command.Parameters.AddWithValue("NOMBREARCHIVO", item.NombreArchivo);
                command.Parameters.AddWithValue("MIMEESPERADO", item.MimeEsperado);
                command.Parameters.AddWithValue("TAMANNOESPERADO", item.TamannoEsperado);
                command.Parameters.AddWithValue("MIMEREAL", (object?)item.MimeReal ?? DBNull.Value);
                command.Parameters.AddWithValue("TAMANNOREAL", (object?)item.TamannoReal ?? DBNull.Value);
                command.Parameters.AddWithValue("ESTADOSUBIDA", item.EstadoSubida);
                command.Parameters.AddWithValue("FECHAEMISIONURLPREFIRMADAPUT", item.FechaEmisionUrlPrefirmadaPut);
                command.Parameters.AddWithValue("FECHACONFIRMACIONSUBIDA", (object?)item.FechaConfirmacionSubida ?? DBNull.Value);
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
