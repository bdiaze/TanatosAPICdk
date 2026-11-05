using System.Text;

namespace TanatosAPI.Helpers {
	public static class CsvGenerator {
		public static void AgregarFila(StringBuilder builder, params string?[] valores) {
			for (int i = 0; i < valores.Length; i++) {
				if (i > 0) 
					builder.Append(';');

				AgregarCampo(builder, valores[i]);
			}
			builder.Append("\r\n");
		}

		private static void AgregarCampo(StringBuilder builder, string? valor) {
			builder.Append('"');

			if (valor is not null) {
				foreach (char caracter in valor) {
					if (caracter == '"')
						builder.Append("\"\"");
					else 
						builder.Append(caracter);
				}
			}

			builder.Append('"');
		}

		public static string? ProtegerTexto(string? valor) {
			if (string.IsNullOrEmpty(valor))
				return valor;

			int indice = 0;
			while (indice < valor.Length && (char.IsWhiteSpace(valor[indice]) || char.IsControl(valor[indice]))) {
				indice++;
			}

			if (indice < valor.Length && valor[indice] is '=' or '+' or '-' or '@') {
				return "'" + valor;
			}

			return valor;
		}
	}
}
