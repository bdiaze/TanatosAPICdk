using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Helpers;

namespace TanatosAPI.Test.Helpers {
	public class FrecuenciaDiasHelperTest {
		private static readonly DateTime FECHA_DUMMY_CHILE = new(2026, 1, 15, 11, 0, 0, DateTimeKind.Unspecified); // UTC: 15-01-2026 14:00 - Jueves
				
		public static TheoryData<int, DateTime, DateTime, DateTime, DateTime, DateTime> OcurrenciasFrecuenciasDias => new() {
			{ 14, FECHA_DUMMY_CHILE, FECHA_DUMMY_CHILE, FECHA_DUMMY_CHILE, FECHA_DUMMY_CHILE.AddDays(14), FECHA_DUMMY_CHILE }, // 14 días con referencia igual a inicio
			{ 14, FECHA_DUMMY_CHILE, FECHA_DUMMY_CHILE.AddDays(3), FECHA_DUMMY_CHILE, FECHA_DUMMY_CHILE.AddDays(14), FECHA_DUMMY_CHILE }, // 14 días con referencia a 3 días de inicio 
			{ 14, FECHA_DUMMY_CHILE, FECHA_DUMMY_CHILE.AddDays(7), FECHA_DUMMY_CHILE, FECHA_DUMMY_CHILE.AddDays(14), FECHA_DUMMY_CHILE }, // 14 días con referenciaa 7 días de inicio
			{ 14, FECHA_DUMMY_CHILE, FECHA_DUMMY_CHILE.AddDays(10), FECHA_DUMMY_CHILE, FECHA_DUMMY_CHILE.AddDays(14), FECHA_DUMMY_CHILE.AddDays(14) }, // 14 días con referenciaa 10 días de inicio
			{ 14, FECHA_DUMMY_CHILE, FECHA_DUMMY_CHILE.AddMonths(3), FECHA_DUMMY_CHILE.AddDays(14 * 6), FECHA_DUMMY_CHILE.AddDays(14 * 7), FECHA_DUMMY_CHILE.AddDays(14 * 6) }, // 14 días con referencia posterior a cambio horario (15 abril 2026)
		};
		[Theory]
		[MemberData(nameof(OcurrenciasFrecuenciasDias))]
		public async Task ObtenerOcurrenciasFrecuenciaDiasTest(int frecuenciaDias, DateTime inicioEjecucionChile, DateTime fechaReferenciaChile, DateTime expectedAnteriorChile, DateTime expectedSiguienteChile, DateTime expectedMasCercanaChile) {
			DateTime inicioEjecucionUtc = DateTimeHelper.TransformarFechaTimezoneAUTC(inicioEjecucionChile);
			DateTime fechaReferenciaUtc = DateTimeHelper.TransformarFechaTimezoneAUTC(fechaReferenciaChile);

			(DateTime? Anterior, DateTime? Siguiente, DateTime MasCercana) retorno = FrecuenciaDiasHelper.ObtenerOcurrenciasFrecuenciaDias(frecuenciaDias, inicioEjecucionUtc, fechaReferenciaUtc);
			Assert.Equal(DateTimeHelper.TransformarFechaTimezoneAUTC(expectedAnteriorChile), retorno.Anterior);
			Assert.Equal(DateTimeHelper.TransformarFechaTimezoneAUTC(expectedSiguienteChile), retorno.Siguiente);
			Assert.Equal(DateTimeHelper.TransformarFechaTimezoneAUTC(expectedMasCercanaChile), retorno.MasCercana);
		}
	}
}
