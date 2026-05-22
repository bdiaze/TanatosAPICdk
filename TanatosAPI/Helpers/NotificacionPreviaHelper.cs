using TanatosAPI.Entities.Models;

namespace TanatosAPI.Helpers {
	public static class NotificacionPreviaHelper {
		public static DateTime ObtenerFechaChileNotificacionPrevia(DateTime fechaReferenciaChile, int cantAntelacion, TipoUnidadTiempo unidadTiempo) {
			DateTime fechaNotificacionChile = fechaReferenciaChile;

			if (unidadTiempo.CantDias != null) {
				long diasPrevios = cantAntelacion * unidadTiempo.CantDias.Value;
				fechaNotificacionChile = fechaNotificacionChile.AddDays(-1 * diasPrevios);
			} else if (unidadTiempo.CantHoras != null) {
				long horasPrevias = cantAntelacion * unidadTiempo.CantHoras.Value;
				fechaNotificacionChile = fechaNotificacionChile.AddHours(-1 * horasPrevias);
			} else if (unidadTiempo.CantMinutos != null) {
				long minutosPrevios = cantAntelacion * unidadTiempo.CantMinutos.Value;
				fechaNotificacionChile = fechaNotificacionChile.AddMinutes(-1 * minutosPrevios);
			} else {
				long segundosPrevios = cantAntelacion * unidadTiempo.CantSegundos;
				fechaNotificacionChile = fechaNotificacionChile.AddSeconds(-1 * segundosPrevios);
			}

			return fechaNotificacionChile;
		}

		public static DateTime ObtenerFechaReferenciaChileSegunNotificacionPrevia(DateTime fechaNotificacionChile, int cantAntelacion, TipoUnidadTiempo unidadTiempo) {
			DateTime fechaReferenciaChile = fechaNotificacionChile;

			if (unidadTiempo.CantDias != null) {
				long diasPosteriores = cantAntelacion * unidadTiempo.CantDias.Value;
				fechaReferenciaChile = fechaReferenciaChile.AddDays(diasPosteriores);
			} else if (unidadTiempo.CantHoras != null) {
				long horasPosteriores = cantAntelacion * unidadTiempo.CantHoras.Value;
				fechaReferenciaChile = fechaReferenciaChile.AddHours(horasPosteriores);
			} else if (unidadTiempo.CantMinutos != null) {
				long minutosPosteriores = cantAntelacion * unidadTiempo.CantMinutos.Value;
				fechaReferenciaChile = fechaReferenciaChile.AddMinutes(minutosPosteriores);
			} else {
				long segundosPosteriores = cantAntelacion * unidadTiempo.CantSegundos;
				fechaReferenciaChile = fechaReferenciaChile.AddSeconds(segundosPosteriores);
			}

			return fechaReferenciaChile;
		}
	}
}
