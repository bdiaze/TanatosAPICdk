using NSubstitute;
using NSubstitute.Core.Arguments;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.UseCases {
	public class CategoriaNormaUseCaseTest {
		private readonly ICategoriaNormaBcp categoriaNormaBcp = Substitute.For<ICategoriaNormaBcp>();
		private readonly CategoriaNormaUseCase categoriaNormaUseCase;

		public CategoriaNormaUseCaseTest() {
			categoriaNormaUseCase = new(categoriaNormaBcp);
		}

		public static CategoriaNorma CategoriaNormaDummy(
			long id = 1,
			string nombre = "NombreTest",
			string? nombreCorto = "NombreCortoTest",
			string? descripcion = "DescripcionTest",
			bool vigencia = true
		) => new() {
			Id = id,
			Nombre = nombre,
			NombreCorto = nombreCorto,
			Descripcion = descripcion,
			Vigencia = vigencia
		};

		[Fact]
		public async Task ObtenerVigentesTest() {
			categoriaNormaBcp.ObtenerVigentes().Returns([
				CategoriaNormaDummy(id: 1, vigencia: true),
				CategoriaNormaDummy(id: 2, vigencia: true)
			]);

			List<CategoriaNorma> categorias = await categoriaNormaUseCase.ObtenerVigentes();
			Assert.All(categorias, categoria => Assert.True(categoria.Vigencia));
			Assert.Equal(2, categorias.Count);
			await categoriaNormaBcp.Received(1).ObtenerVigentes();
		}

		[Theory]
		[InlineData(true, true, 2)]
		[InlineData(false, false, 1)]
		[InlineData(null, null, 3)]
		public async Task ObtenerPorVigenciaTest(bool? vigencia, bool? expectedVigencia, int expectedCount) {
			categoriaNormaBcp.ObtenerPorVigencia(true).Returns([
				CategoriaNormaDummy(id: 1, vigencia: true),
				CategoriaNormaDummy(id: 2, vigencia: true)
			]);
			categoriaNormaBcp.ObtenerPorVigencia(false).Returns([
				CategoriaNormaDummy(id: 3, vigencia: false),
			]);
			categoriaNormaBcp.ObtenerPorVigencia(null).Returns([
				CategoriaNormaDummy(id: 1, vigencia: true),
				CategoriaNormaDummy(id: 2, vigencia: true),
				CategoriaNormaDummy(id: 3, vigencia: false),
			]);

			List<CategoriaNorma> categorias = await categoriaNormaUseCase.ObtenerPorVigencia(vigencia);
			if (expectedVigencia != null) Assert.All(categorias, categoria => Assert.Equal(expectedVigencia, categoria.Vigencia));
			Assert.Equal(expectedCount, categorias.Count);
			await categoriaNormaBcp.Received(1).ObtenerPorVigencia(expectedVigencia);
		}

		[Fact]
		public async Task RegistrarCategoriaTest_NuevaCategoria() {
			categoriaNormaBcp.ObtenerPorId(Arg.Any<long>()).Returns((CategoriaNorma?)null);
			categoriaNormaBcp.RegistrarCategoria(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(
				CategoriaNormaDummy(id: 1, nombre: "Nombre", nombreCorto: "NombreCorto", descripcion: "Descripcion", vigencia: true)	
			);

			CategoriaNorma categoria = await categoriaNormaUseCase.RegistrarCategoria(1, "Nombre", "NombreCorto", "Descripcion", true);

			Assert.Equal(1, categoria.Id);
			Assert.Equal("Nombre", categoria.Nombre);
			Assert.Equal("NombreCorto", categoria.NombreCorto);
			Assert.Equal("Descripcion", categoria.Descripcion);
			Assert.True(categoria.Vigencia);
			await categoriaNormaBcp.Received(1).RegistrarCategoria(1, "Nombre", "NombreCorto", "Descripcion", true);
		}

		[Fact]
		public async Task RegistrarCategoriaTest_IdYaUsado() {
			categoriaNormaBcp.ObtenerPorId(1).Returns(CategoriaNormaDummy(id: 1, nombre: "UnNombreCualquiera"));

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => categoriaNormaUseCase.RegistrarCategoria(1, "UnNombreDistinto", "UnNombreCorto", "UnaDescripcion", true));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);
		}

		[Fact]
		public async Task RegistrarCategoriaTest_YaRegistrada() {
			CategoriaNorma existente = CategoriaNormaDummy();
			categoriaNormaBcp.ObtenerPorId(existente.Id).Returns(existente);

			CategoriaNorma categoria = await categoriaNormaUseCase.RegistrarCategoria(
				existente.Id,
				existente.Nombre,
				existente.NombreCorto,
				existente.Descripcion,
				existente.Vigencia
			);

			Assert.Equal(existente.Id, categoria.Id);
			Assert.Equal(existente.Nombre, categoria.Nombre);
			Assert.Equal(existente.NombreCorto, categoria.NombreCorto);
			Assert.Equal(existente.Descripcion, categoria.Descripcion);
			Assert.Equal(existente.Vigencia, categoria.Vigencia);
			await categoriaNormaBcp.DidNotReceive().RegistrarCategoria(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>());
		}

		[Fact]
		public async Task ActualizarCategoriaTest_NoExistente() {
			categoriaNormaBcp.ObtenerPorId(Arg.Any<long>()).Returns((CategoriaNorma?)null);

			CategoriaNorma noExistente = CategoriaNormaDummy();

			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => categoriaNormaUseCase.ActualizarCategoria(
				noExistente.Id, noExistente.Nombre, noExistente.NombreCorto, noExistente.Descripcion, noExistente.Vigencia
			));
			Assert.Equal(TipoErrorValidacion.ValorNoValido, ex.TipoErrorValidacion);
		}

		[Fact]
		public async Task ActualizarCategoriaTest_YaRegistrada() {
			CategoriaNorma existente = CategoriaNormaDummy();
			categoriaNormaBcp.ObtenerPorId(existente.Id).Returns(existente);

			CategoriaNorma categoria = await categoriaNormaUseCase.ActualizarCategoria(
				existente.Id,
				existente.Nombre,
				existente.NombreCorto,
				existente.Descripcion,
				existente.Vigencia
			);

			Assert.Equal(existente.Id, categoria.Id);
			Assert.Equal(existente.Nombre, categoria.Nombre);
			Assert.Equal(existente.NombreCorto, categoria.NombreCorto);
			Assert.Equal(existente.Descripcion, categoria.Descripcion);
			Assert.Equal(existente.Vigencia, categoria.Vigencia);
			await categoriaNormaBcp.DidNotReceive().ActualizarCategoria(Arg.Any<CategoriaNorma>());
		}

		[Fact]
		public async Task ActualizarCategoriaTest_Existente() {
			CategoriaNorma existente = CategoriaNormaDummy();

			categoriaNormaBcp.ObtenerPorId(existente.Id).Returns(existente);

			CategoriaNorma categoria = await categoriaNormaUseCase.ActualizarCategoria(existente.Id, "DistintoNombre", "DistintoNombreCorto", "DistintaDescripcion", true);

			Assert.Equal(1, categoria.Id);
			Assert.Equal("DistintoNombre", categoria.Nombre);
			Assert.Equal("DistintoNombreCorto", categoria.NombreCorto);
			Assert.Equal("DistintaDescripcion", categoria.Descripcion);
			Assert.True(categoria.Vigencia);
			await categoriaNormaBcp.Received(1).ActualizarCategoria(Arg.Is<CategoriaNorma>(c =>
				c.Id == existente.Id &&
				c.Nombre == "DistintoNombre" &&
				c.NombreCorto == "DistintoNombreCorto" &&
				c.Descripcion == "DistintaDescripcion" &&
				c.Vigencia == true
			));
		}

		[Fact]
		public async Task EliminarCategoriaTest_Existente() {
			CategoriaNorma existente = CategoriaNormaDummy();

			categoriaNormaBcp.ObtenerPorId(existente.Id).Returns(existente);

			await categoriaNormaUseCase.EliminarCategoria(existente.Id);
			await categoriaNormaBcp.Received(1).EliminarCategoria(existente.Id);
		}

		[Fact]
		public async Task EliminarCategoriaTest_NoExistente() {
			categoriaNormaBcp.ObtenerPorId(Arg.Any<long>()).Returns((CategoriaNorma?)null);

			await categoriaNormaUseCase.EliminarCategoria(1);
			await categoriaNormaBcp.DidNotReceive().EliminarCategoria(Arg.Any<long>());
		}
	}
}
