namespace TanatosAPI.Helpers {
	public static class DateTimeHelper {
		public static DateTime TransformarFechaUTCATimezone(DateTime fecha, string timezone = "America/Santiago") {
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
			
			// Nos aseguramos de que la fecha esté en UTC...
			fecha = DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
			return TimeZoneInfo.ConvertTimeFromUtc(fecha, timeZoneInfo);
		}

		public static DateTime TransformarFechaTimezoneAUTC(DateTime fecha, string timezone = "America/Santiago") {
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);

			// Especificamos que la fecha es local (de esa timezone)
			fecha = DateTime.SpecifyKind(fecha, DateTimeKind.Unspecified);
			return TimeZoneInfo.ConvertTimeToUtc(fecha, timeZoneInfo);
		}

		public static DateTime SumarMeses(DateTime fechaUtc, int meses, string timezone = "America/Santiago") {
			if (fechaUtc.Kind != DateTimeKind.Utc) throw new InvalidOperationException("La fecha debe estar en UTC");

			DateTime fechaChile = TransformarFechaUTCATimezone(fechaUtc, timezone);
			fechaChile = fechaChile.AddMonths(meses);
			return TransformarFechaTimezoneAUTC(fechaChile, timezone);
		}
	}
}
