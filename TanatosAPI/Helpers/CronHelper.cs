using System.Text.RegularExpressions;
using TimeZoneConverter;

namespace TanatosAPI.Helpers {
	public static class CronHelper {
		private static string DayOfWeekToCronValue(DayOfWeek dayOfWeek) {
			return dayOfWeek switch {
				DayOfWeek.Sunday => "SUN",
				DayOfWeek.Monday => "MON",
				DayOfWeek.Tuesday => "TUE",
				DayOfWeek.Wednesday => "WED",
				DayOfWeek.Thursday => "THU",
				DayOfWeek.Friday => "FRI",
				DayOfWeek.Saturday => "SAT",
				_ => throw new ArgumentException("Día de la semana inválido"),
			};
		}

		public static string GenerarCronAWSDesdeFecha(DateTime fecha, string? baseCronAWS = null) {
			baseCronAWS = baseCronAWS?.Trim();
			if (baseCronAWS != null) baseCronAWS = Regex.Replace(baseCronAWS, @"\s+", " ", RegexOptions.NonBacktracking);
			baseCronAWS ??= "MI HO DM MO ? YE";

			// Se desglosa la fecha en sus elementos...
			string[] elementosFecha = [
				fecha.Minute.ToString(),				// MI
				fecha.Hour.ToString(),					// HO
				fecha.Day.ToString(),					// DM
				fecha.Month.ToString(),					// MO
				DayOfWeekToCronValue(fecha.DayOfWeek),	//DW
				fecha.Year.ToString()					// YE
			];

			// Se desglosa el base cron en sus elementos...
			string[] cronConfigs = baseCronAWS.Split(' ');
			if (cronConfigs.Length != 6) throw new ArgumentException("baseCron inválido");

			// Se edita cada elemento del cron según la configuración...
			for (int i = 0; i < 6; i++) {
				// Si la configuración del elemento cron contiene múltiples opciones separadas por '|', se selecciona la opción que contiene el valor correspondiente de la fecha...
				if (cronConfigs[i].Contains('|')) {
					string[] opciones = cronConfigs[i].Split('|');
					foreach (string opcion in opciones) {
						string[] valoresOpcion = opcion.Split(',');
						if (valoresOpcion.Contains(elementosFecha[i])) {
							cronConfigs[i] = string.Join(",", valoresOpcion);
							break;
						}
					}
				// Si la configuración del elemento cron es un marcador de posición, se reemplaza por el valor correspondiente de la fecha...
				} else if (cronConfigs[i] == "MI") {
					cronConfigs[i] = elementosFecha[0];
				} else if (cronConfigs[i] == "HO") {
					cronConfigs[i] = elementosFecha[1];
				} else if (cronConfigs[i] == "DM") {
					cronConfigs[i] = elementosFecha[2];
				} else if (cronConfigs[i] == "MO") {
					cronConfigs[i] = elementosFecha[3];
				} else if (cronConfigs[i] == "DW") {
					cronConfigs[i] = elementosFecha[4];
				} else if (cronConfigs[i] == "YE") {
					cronConfigs[i] = elementosFecha[5];
				}
			}

			return string.Join(" ", cronConfigs);
		}

		public static DateTime TransformarFechaUTCATimezone(DateTime fecha, string timezone = "America/Santiago") {
			// Nos aseguramos de que la fecha esté en UTC...
			fecha = DateTime.SpecifyKind(fecha, DateTimeKind.Utc);

			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);

			return TimeZoneInfo.ConvertTimeFromUtc(fecha, timeZoneInfo);
		}

		public static DateTime TransformarFechaTimezoneAUTC(DateTime fecha, string timezone = "America/Santiago") {
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);

			// Especificamos que la fecha es local (de esa timezone)
			fecha = DateTime.SpecifyKind(fecha, DateTimeKind.Unspecified);

			return TimeZoneInfo.ConvertTimeToUtc(fecha, timeZoneInfo);
		}

		public static string TransformarCronAWSAStandard(string awsCron) {
			awsCron = awsCron.Trim();
			awsCron = Regex.Replace(awsCron, @"\s+", " ", RegexOptions.NonBacktracking);
			string[] campos = awsCron.Split(' ');
			return string.Join(' ', campos[..5].Select(f => f.Replace("?", "*")));
		}
	}
}
