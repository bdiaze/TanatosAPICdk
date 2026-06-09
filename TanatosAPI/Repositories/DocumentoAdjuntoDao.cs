using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;

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

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<DocumentoAdjunto> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new DocumentoAdjunto {
						Id = reader.GetInt64(0),
						IdHistorialNormaSuscrita = reader.GetInt64(1),
						BucketName = reader.GetString(2),
						BucketKey = reader.GetString(3),
						NombreArchivo = reader.GetString(4),
						MimeEsperado = reader.GetString(5),
						TamannoEsperado = reader.GetInt64(6),
						MimeReal = await reader.IsDBNullAsync(7) ? null : reader.GetString(7),
						TamannoReal = await reader.IsDBNullAsync(8) ? null : reader.GetInt64(8),
						EstadoSubida = reader.GetInt16(9),
						FechaEmisionUrlPrefirmadaPut = reader.GetDateTime(10),
						FechaConfirmacionSubida = await reader.IsDBNullAsync(11) ? null : reader.GetDateTime(11),
						FechaCreacion = reader.GetDateTime(12),
						FechaEliminacion = await reader.IsDBNullAsync(13) ? null : reader.GetDateTime(13),
						Vigencia = reader.GetBoolean(14)
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

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				DocumentoAdjunto? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new DocumentoAdjunto {
						Id = reader.GetInt64(0),
						IdHistorialNormaSuscrita = reader.GetInt64(1),
						BucketName = reader.GetString(2),
						BucketKey = reader.GetString(3),
						NombreArchivo = reader.GetString(4),
						MimeEsperado = reader.GetString(5),
						TamannoEsperado = reader.GetInt64(6),
						MimeReal = await reader.IsDBNullAsync(7) ? null : reader.GetString(7),
						TamannoReal = await reader.IsDBNullAsync(8) ? null : reader.GetInt64(8),
						EstadoSubida = reader.GetInt16(9),
						FechaEmisionUrlPrefirmadaPut = reader.GetDateTime(10),
						FechaConfirmacionSubida = await reader.IsDBNullAsync(11) ? null : reader.GetDateTime(11),
						FechaCreacion = reader.GetDateTime(12),
						FechaEliminacion = await reader.IsDBNullAsync(13) ? null : reader.GetDateTime(13),
						Vigencia = reader.GetBoolean(14)
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
                command.Parameters.AddWithValue("IDHISTORIALNORMASUSCRITA", item.IdHistorialNormaSuscrita);
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
                command.Parameters.AddWithValue("IDHISTORIALNORMASUSCRITA", item.IdHistorialNormaSuscrita);
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
