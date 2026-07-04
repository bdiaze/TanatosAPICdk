using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;

namespace TanatosAPI.UseCases {
	public class VideoTutorialUseCase(IVideoTutorialBcp videoTutorialBcp) {
		public async Task<List<VideoTutorial>> ObtenerHabilitados() {
			List<VideoTutorial> vigentes = await videoTutorialBcp.ObtenerVigentes();
			return [.. vigentes.Where(p => p.Habilitado)];
		}

		public async Task<List<VideoTutorial>> ObtenerVigentes() {
			return await videoTutorialBcp.ObtenerVigentes();
		}

		public async Task<VideoTutorial> Registrar(string titulo, string? descripcion, string url, bool habilitado, int orden) {
			titulo = titulo.Trim();
			descripcion = descripcion?.Trim();
			url = url.Trim();

			if (orden <= 0) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "El video tutorial no puede tener un orden menor o igual a 0.");
			}

			List<VideoTutorial> vigentes = await ObtenerVigentes();
			if (vigentes.Any(p => p.Titulo.Equals(titulo, StringComparison.OrdinalIgnoreCase))) {
				throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya existe dicho video tutorial.");
			}

			if (vigentes.Any(p => p.Orden == orden)) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Ya existe un video tutorial con dicho orden.");
			}

			return await videoTutorialBcp.Insertar(titulo, descripcion, url, habilitado, orden);
		}

		public async Task<VideoTutorial> Actualizar(long id, string titulo, string? descripcion, string url, bool habilitado, int orden) {
			titulo = titulo.Trim();
			descripcion = descripcion?.Trim();
			url = url.Trim();

			if (orden <= 0) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "El video tutorial no puede tener un orden menor o igual a 0.");
			}

			List<VideoTutorial> vigentes = await ObtenerVigentes();
			VideoTutorial existente = vigentes.FirstOrDefault(p => p.Id == id) ?? throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El video tutorial no existe o no está vigente", "No existe dicho video tutorial.");

			if (vigentes.Any(p => p.Id != id && p.Titulo.Equals(titulo, StringComparison.OrdinalIgnoreCase))) {
				throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya existe dicho video tutorial.");
			}

			if (vigentes.Any(p => p.Id != id && p.Orden == orden)) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Ya existe un video tutorial con dicho orden.");
			}

			if (existente.Titulo != titulo || existente.Descripcion != descripcion || existente.Url != url || existente.Habilitado != habilitado || existente.Orden != orden) {
				existente.Titulo = titulo;
				existente.Descripcion = descripcion;
				existente.Url = url;
				existente.Habilitado = habilitado;
				existente.Orden = orden;

				await videoTutorialBcp.Modificar(existente);
			}

			return existente;
		}

		public async Task Eliminar(long idVideoTutorial) {
			List<VideoTutorial> vigentes = await ObtenerVigentes();
			VideoTutorial? existente = vigentes.FirstOrDefault(p => p.Id == idVideoTutorial);

			if (!videoTutorialBcp.EstaVigente(existente)) {
				return;
			}

			await videoTutorialBcp.Eliminar(existente!);
		}
	}
}
