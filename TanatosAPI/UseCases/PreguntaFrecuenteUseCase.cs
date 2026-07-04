using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;

namespace TanatosAPI.UseCases {
	public class PreguntaFrecuenteUseCase(IPreguntaFrecuenteBcp preguntaFrecuenteBcp) {
		public async Task<List<PreguntaFrecuente>> ObtenerHabilitados() {
			List<PreguntaFrecuente> vigentes = await preguntaFrecuenteBcp.ObtenerVigentes();
			return [.. vigentes.Where(p => p.Habilitado)];
		}
		public async Task<List<PreguntaFrecuente>> ObtenerVigentes() {
			return await preguntaFrecuenteBcp.ObtenerVigentes();
		}

		public async Task<PreguntaFrecuente> Registrar(string pregunta, string respuesta, bool habilitado, int orden) {
			pregunta = pregunta.Trim();
			respuesta = respuesta.Trim();

			if (orden <= 0) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "La pregunta frecuente no puede tener un orden menor o igual a 0.");
			}

			List<PreguntaFrecuente> vigentes = await ObtenerVigentes();
			if (vigentes.Any(p => p.Pregunta.Equals(pregunta, StringComparison.OrdinalIgnoreCase))) {
				throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya existe dicha pregunta frecuente.");
			}

			if (vigentes.Any(p => p.Orden == orden)) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Ya existe una pregunta frecuente con dicho orden.");
			}

			return await preguntaFrecuenteBcp.Insertar(pregunta, respuesta, habilitado, orden);
		}

		public async Task<PreguntaFrecuente> Actualizar(long id, string pregunta, string respuesta, bool habilitado, int orden) {
			pregunta = pregunta.Trim();
			respuesta = respuesta.Trim();

			if (orden <= 0) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "La pregunta frecuente no puede tener un orden menor o igual a 0.");
			}

			List<PreguntaFrecuente> vigentes = await ObtenerVigentes();
			PreguntaFrecuente existente = vigentes.FirstOrDefault(p => p.Id == id) ?? throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "La pregunta frecuente no existe o no está vigente", "No existe dicha pregunta frecuente.");
			
			if (vigentes.Any(p => p.Id != id && p.Pregunta.Equals(pregunta, StringComparison.OrdinalIgnoreCase))) {
				throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya existe dicha pregunta frecuente.");
			}

			if (vigentes.Any(p => p.Id != id && p.Orden == orden)) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Ya existe una pregunta frecuente con dicho orden.");
			}

			if (existente.Pregunta != pregunta || existente.Respuesta != respuesta || existente.Habilitado != habilitado || existente.Orden != orden) {
				existente.Pregunta = pregunta;
				existente.Respuesta = respuesta;
				existente.Habilitado = habilitado;
				existente.Orden = orden;

				await preguntaFrecuenteBcp.Modificar(existente);
			}

			return existente;
		}

		public async Task Eliminar(long idPreguntaFrecuente) {
			List<PreguntaFrecuente> vigentes = await ObtenerVigentes();
			PreguntaFrecuente? existente = vigentes.FirstOrDefault(p => p.Id == idPreguntaFrecuente);

			if (!preguntaFrecuenteBcp.EstaVigente(existente)) {
				return;
			}

			await preguntaFrecuenteBcp.Eliminar(existente!);
		}
	}
}
