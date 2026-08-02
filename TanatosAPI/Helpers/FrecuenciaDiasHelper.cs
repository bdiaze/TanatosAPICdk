using AwsArnParsing_Compile;

namespace TanatosAPI.Helpers {
	public static class FrecuenciaDiasHelper {
		public static (DateTime? anteriorUtc, DateTime? siguienteUtc, DateTime masCercanaUtc) ObtenerOcurrenciasFrecuenciaDias(int frecuenciaDias, DateTime inicioEjecucionUtc, DateTime fechaReferenciaUtc) {
			DateTime fechaReferenciaChile = DateTimeHelper.TransformarFechaUTCATimezone(fechaReferenciaUtc);
			DateTime inicioEjecucionChile = DateTimeHelper.TransformarFechaUTCATimezone(inicioEjecucionUtc);

			double cantFrecuencias = (fechaReferenciaChile - inicioEjecucionChile).TotalDays / frecuenciaDias;
			int cantFrecuenciasInf = (int)Math.Floor(cantFrecuencias);
			int cantFrecuenciasSup = cantFrecuenciasInf + 1;
			DateTime fechaAnteriorChile = inicioEjecucionChile.AddDays(frecuenciaDias * cantFrecuenciasInf);
			DateTime fechaSiguienteChile = inicioEjecucionChile.AddDays(frecuenciaDias * cantFrecuenciasSup);

			DateTime? anteriorUTC = DateTimeHelper.TransformarFechaTimezoneAUTC(fechaAnteriorChile);
			DateTime? siguienteUTC = DateTimeHelper.TransformarFechaTimezoneAUTC(fechaSiguienteChile);
			DateTime masCercanaUTC = (siguienteUTC, anteriorUTC) switch {
				(null, null) => throw new InvalidOperationException($"La frecuencia de {frecuenciaDias} días con inicio en {inicioEjecucionUtc:O} no tiene ocurrencias válidas."),
				(null, _) => anteriorUTC!.Value,
				(_, null) => siguienteUTC!.Value,
				_ => (siguienteUTC!.Value - fechaReferenciaUtc) < (fechaReferenciaUtc - anteriorUTC!.Value) ? siguienteUTC!.Value : anteriorUTC!.Value
			};
			return (anteriorUTC, siguienteUTC, masCercanaUTC);
		}
	}
}
