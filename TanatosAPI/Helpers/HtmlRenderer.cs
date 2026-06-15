using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace TanatosAPI.Helpers {
    public class HtmlRenderer(IHostEnvironment environment) {
        private async Task<string> ObtenerTemplate(string nombreTemplate) {
			string path;
			if (!environment.IsDevelopment()) {
				path = Path.Combine(AppContext.BaseDirectory, "TemplatesCorreos", nombreTemplate);
			} else {
				path = Path.Combine(Directory.GetCurrentDirectory(), "TemplatesCorreos", nombreTemplate);
			}
            return await File.ReadAllTextAsync(path);
		}

        public async Task<string> GenerarHtml(string nombreTemplate, ScriptObject? parametros = null, bool conTemplateBase = true) {
            string strTemplate = await ObtenerTemplate(nombreTemplate);
            Template template = Template.Parse(strTemplate);
            TemplateContext context = new();
            if (parametros != null) {
                context.PushGlobal(parametros);
            }

            string contenido = await template.RenderAsync(context);

            if (conTemplateBase) {
                string strTemplateBase = await ObtenerTemplate("TemplateBase.html");
				
				Template templateBase = Template.Parse(strTemplateBase);
				ScriptObject objectTemplateBase = new() {
					["CONTENIDO"] = contenido
				};
                TemplateContext contextBase = new();
				contextBase.PushGlobal(objectTemplateBase);
                return await templateBase.RenderAsync(contextBase);
			}


			return contenido;
        }
    }
}
