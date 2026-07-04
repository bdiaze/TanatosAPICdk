using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using NSubstitute;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TanatosAPI.Entities.Others.Flow;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Test.Helpers {
	public class HtmlRendererTest {
		private readonly IHostEnvironment env = Substitute.For<IHostEnvironment>();
		private readonly IFileHelper fileHelper = Substitute.For<IFileHelper>();
		private readonly HtmlRenderer htmlRenderer;

		public HtmlRendererTest() {
			htmlRenderer = new HtmlRenderer(env, fileHelper);
		}

		[Fact]
		public async Task GenerarHtml_ValidoSinParametrosSinTemplateBase() {
			env.EnvironmentName = Environments.Production;
			fileHelper.ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("nombre-template-test"))).Returns("Template de test");

			string retorno = await htmlRenderer.GenerarHtml("nombre-template-test", null, false);
			Assert.Equal("Template de test", retorno);
			await fileHelper.Received(1).ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("nombre-template-test")));
			await fileHelper.DidNotReceive().ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("TemplateBase.html")));
		}

		[Fact]
		public async Task GenerarHtml_ValidoSinParametrosConTemplateBase() {
			env.EnvironmentName = Environments.Production;
			fileHelper.ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("nombre-template-test"))).Returns("Template de test");
			fileHelper.ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("TemplateBase.html"))).Returns("Template {{ CONTENIDO }} base");

			string retorno = await htmlRenderer.GenerarHtml("nombre-template-test");
			Assert.Equal("Template Template de test base", retorno);
			await fileHelper.Received(1).ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("nombre-template-test")));
			await fileHelper.Received(1).ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("TemplateBase.html")));
		}

		[Fact]
		public async Task GenerarHtml_ValidoConParametrosConTemplateBase() {
			env.EnvironmentName = Environments.Production;
			fileHelper.ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("nombre-template-test"))).Returns("Template {{ PARAMETRO }} test");
			fileHelper.ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("TemplateBase.html"))).Returns("Template {{ CONTENIDO }} base");

			string retorno = await htmlRenderer.GenerarHtml("nombre-template-test", new ScriptObject() {
				["PARAMETRO"] = "valor-parametro",
			});
			Assert.Equal("Template Template valor-parametro test base", retorno);
			await fileHelper.Received(1).ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("nombre-template-test")));
			await fileHelper.Received(1).ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("TemplateBase.html")));
		}

		[Fact]
		public async Task GenerarHtml_ValidoConParametrosConTemplateBaseDevelopment() {
			env.EnvironmentName = Environments.Development;
			fileHelper.ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("nombre-template-test"))).Returns("Template {{ PARAMETRO }} test");
			fileHelper.ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("TemplateBase.html"))).Returns("Template {{ CONTENIDO }} base");

			string retorno = await htmlRenderer.GenerarHtml("nombre-template-test", new ScriptObject() {
				["PARAMETRO"] = "valor-parametro",
			});
			Assert.Equal("Template Template valor-parametro test base", retorno);
			await fileHelper.Received(1).ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("nombre-template-test")));
			await fileHelper.Received(1).ReadAllTextAsync(Arg.Is<string>(s => s.EndsWith("TemplateBase.html")));
		}
	}
}
