using System.Text.RegularExpressions;

namespace TanatosAPI.Helpers {
	public static class CronHelper {
		public static string GenerarCronDesdeFecha(DateTime fecha, string? baseCron = null) {
			baseCron = baseCron?.Trim();
			if (baseCron != null) baseCron = Regex.Replace(baseCron, @"\s+", " ");
			baseCron ??= "MI HO DM MO ? YE";

			// Se desglosa la fecha en sus elementos...
			string[] elementosFecha = [
				fecha.Minute.ToString(),			// MI
				fecha.Hour.ToString(),				// HO
				fecha.Day.ToString(),				// DM
				fecha.Month.ToString(),				// MO
				((int)fecha.DayOfWeek).ToString(),	// DW
				fecha.Year.ToString()				// YE
			];

			// Se desglosa el base cron en sus elementos...
			string[] cronConfigs = baseCron.Split(' ');
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
	}
}
