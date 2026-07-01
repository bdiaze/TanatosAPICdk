using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class VideoTutorialBcp(IDateTimeProvider dateTimeProvider, IVideoTutorialDao videoTutorialDao) : IVideoTutorialBcp {
		public bool EstaVigente(VideoTutorial? videoTutorial) {
			return videoTutorial != null && videoTutorial.Vigencia;
		}

		public async Task<List<VideoTutorial>> ObtenerVigentes(NpgsqlTransaction? transaction = null) {
			return await videoTutorialDao.ObtenerPorVigencia(true, transaction);
		}

		public async Task<VideoTutorial> Insertar(string titulo, string? descripcion, string url, bool habilitado, int orden, NpgsqlTransaction? transaction = null) {
			VideoTutorial nuevo = new() {
				Id = 0,
				Titulo = titulo,
				Descripcion = descripcion,
				Url = url,
				Habilitado = habilitado,
				Orden = orden,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await videoTutorialDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Modificar(VideoTutorial videoTutorial, NpgsqlTransaction? transaction = null) {
			await videoTutorialDao.Actualizar(videoTutorial, transaction);
		}

		public async Task Eliminar(VideoTutorial videoTutorial, NpgsqlTransaction? transaction = null) {
			if (videoTutorial.Vigencia) {
				videoTutorial.FechaEliminacion = dateTimeProvider.UtcNow;
				videoTutorial.Vigencia = false;
				await videoTutorialDao.Actualizar(videoTutorial, transaction);
			}
		}
	}
}
