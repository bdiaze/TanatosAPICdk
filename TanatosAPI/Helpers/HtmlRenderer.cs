using Scriban;
using Scriban.Runtime;

namespace TanatosAPI.Helpers {
    public class HtmlRenderer(IHostEnvironment environment) {
        public async Task<string> GenerarHtml(string nombreTemplate, ScriptObject? parametros = null) {
            string path;
            if (!environment.IsDevelopment()) {
                path = Path.Combine(AppContext.BaseDirectory, "TemplatesCorreos", nombreTemplate);
            } else {
                path = Path.Combine(Directory.GetCurrentDirectory(), "TemplatesCorreos", nombreTemplate);
            }

            string strTemplate = await File.ReadAllTextAsync(path);
            Template template = Template.Parse(strTemplate);
            TemplateContext context = new();
            if (parametros != null) {
                context.PushGlobal(parametros);
            }

            return await template.RenderAsync(context);
        }
    }
}
