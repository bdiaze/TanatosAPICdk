using Scriban.Runtime;

namespace TanatosAPI.Interfaces.Helpers {
	public interface IHtmlRenderer {
		public Task<string> GenerarHtml(string nombreTemplate, ScriptObject? parametros = null, bool conTemplateBase = true);
	}
}
